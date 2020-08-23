using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Shared;

namespace ProjectMembersService
{
    public class MoveTaskTransactionHandler : TransactionHandlerBase
    {
        private readonly ProjectMembersRepository _projectMembersRepository;
        private readonly ProjectsRepository _projectsRepository;
        private readonly UsersRepository _usersRepository;

        public MoveTaskTransactionHandler(ProjectMembersRepository projectMembersRepository, ProjectsRepository projectsRepository,
            UsersRepository usersRepository, RequestsRepository requestsRepository, TransactionsRepository transactionsRepository)
            : base(requestsRepository, transactionsRepository)
        {
            _projectMembersRepository = projectMembersRepository;
            _projectsRepository = projectsRepository;
            _usersRepository = usersRepository;
        }

        protected override Task<string> HandleMessageInternalAsync(string messageString, string messageAction)
        {
            switch(messageAction)
            {
                case TransactionMessageActions.MoveTask_MoveMembersRequested:
                    return MoveMembersToProjectAsync(messageString);

                default:
                    throw new Exception($"unknown message action {messageAction}");
            }
        }

        private async Task<string> MoveMembersToProjectAsync(string messageString)
        {
            Console.WriteLine("Prepare members stage started");
            MoveTaskMoveMembersMessage message = JsonConvert.DeserializeObject<MoveTaskMoveMembersMessage>(messageString);

            string transactionId = message.TransactionId;
            var transaction = await _transactionsRepository.GetTransactionAsync(transactionId);

            if(transaction != null && transaction.State == TransactionStates.Denied)
            {
                return $"Transaction {transactionId} already denied";
            }

            IEnumerable<string> taskMembers = message.TaskMembers;
            
            var moveTransaction = transaction == null
                ? MoveTaskTransaction.Create(transactionId, message.TargetProjectId)
                : MoveTaskTransaction.CreateFromBase(transaction);

            moveTransaction.State = TransactionStates.Processing;
            
            if(taskMembers.IsNullOrEmpty())
            {
                // nothing to move
                await HandleNoMembersToMoveAsync(moveTransaction);
                return $"No task members to move";
            }            

            //Check project
            string projectId = message.TargetProjectId;
            var project = await _projectsRepository.GetProjectAsync(projectId);
            if(project == null)
            {
                string reason = $"Project {projectId} not found";                

                var rollbackOutboxMessage = OutboxMessageModel.Create(
                    new RollbackTransactionMessage
                    {
                        TransactionId = transactionId,
                        Reason = reason
                    }, Topics.ProjectMembers, TransactionMessageActions.MoveTask_Rollback);

                moveTransaction.State = TransactionStates.Denied;
                moveTransaction.Message = reason;
                moveTransaction.UpdateData();                   

                await _transactionsRepository.CreateOrUpdateTransactionAsync(moveTransaction, rollbackOutboxMessage);

                return reason;
            }


            var targetProjectMembers = await _projectMembersRepository.GetProjectsMembersAsync(projectId: projectId);
            var targetMembersIds = targetProjectMembers?.Select(m => m.UserId).ToList() ?? new List<string>();
            var movingMembersIds = taskMembers.Except(targetMembersIds).ToList();

            if(!movingMembersIds.Any())
            {
                await HandleNoMembersToMoveAsync(moveTransaction);
                return $"All members already in target";
            }

            var allUsersById = (await _usersRepository.GetAllUsersAsync()).ToDictionary(u => u.Id);
            var allUsersIds = allUsersById.Keys.ToList();

            var notFoundMembers = movingMembersIds.Except(allUsersIds).ToList();

            // Check all members exists
            if(notFoundMembers.Any())
            {
                string reason = $"Members {string.Join(", ", notFoundMembers)} not found";                

                var rollbackOutboxMessage = OutboxMessageModel.Create(
                    new RollbackTransactionMessage
                    {
                        TransactionId = transactionId,
                        Reason = reason
                    }, Topics.ProjectMembers, TransactionMessageActions.MoveTask_Rollback);

                moveTransaction.State = TransactionStates.Denied;
                moveTransaction.Message = reason;
                moveTransaction.UpdateData();                   

                await _transactionsRepository.CreateOrUpdateTransactionAsync(moveTransaction, rollbackOutboxMessage);

                return reason;
            }

            var sourceProjectMembersById = (await _projectMembersRepository.GetProjectsMembersAsync(projectId: message.SourceProjectId))
                .ToDictionary(m => m.UserId);


            //probably will be better rollback this insterts in case of exception
            foreach(var movingMemberId in movingMembersIds)
            {
                ProjectMemberRole role = ProjectMemberRole.Implementer;

                //try use role from source project
                if(sourceProjectMembersById.TryGetValue(movingMemberId, out var sourceProjectMember))
                {
                    role = sourceProjectMember.Role;
                }

                var projectMemberModel = new ProjectMemberModel
                {
                    UserId = movingMemberId,
                    ProjectId = projectId,
                    Role = role
                };

                var outboxMessage = OutboxMessageModel.Create(
                    new ProjectMemberCreatedUpdatedMessage
                    {
                        Role = role,
                        UserId = movingMemberId,
                        Username = allUsersById[movingMemberId].Username,
                        ProjectId = projectId,
                        ProjectTitle = project.Title
                    }, Topics.ProjectMembers, MessageActions.Created);            

                await _projectMembersRepository.AddMemberToProjectAsync(projectMemberModel, outboxMessage);
            }

            moveTransaction.MovedMembers = movingMembersIds.ToList();
            moveTransaction.UpdateData();

            var movedOutboxMessage = OutboxMessageModel.Create(
                new MoveTaskMembersMovedMessage
                {
                    TransactionId = transactionId,                    
                }, Topics.ProjectMembers, TransactionMessageActions.MoveTask_MoveMembersCompleted);

            await _transactionsRepository.CreateOrUpdateTransactionAsync(moveTransaction, movedOutboxMessage);

            return $"Moving task members completed successful";      
        }

        private Task HandleNoMembersToMoveAsync(MoveTaskTransaction moveTaskTransaction)
        {
            moveTaskTransaction.UpdateData();

            var outboxMessage = OutboxMessageModel.Create(
                new MoveTaskMembersMovedMessage
                {
                    TransactionId = moveTaskTransaction.Id,
                }, Topics.ProjectMembers, TransactionMessageActions.MoveTask_MoveMembersCompleted);

            return _transactionsRepository.CreateOrUpdateTransactionAsync(moveTaskTransaction, outboxMessage);
        }

        protected override async Task RollbackTransactionInternalAsync(TransactionBase transaction)
        {
            Console.WriteLine("Rollback started");

            var moveTransaction = MoveTaskTransaction.CreateFromBase(transaction);
            if(moveTransaction.MovedMembers.IsNullOrEmpty())
            {
                Console.WriteLine("Nothing to rollback");
                return;
            }

            string projectId = moveTransaction.ProjectId;

            var projectMembersById = (await _projectMembersRepository.GetProjectsMembersAsync(projectId: projectId)).ToDictionary(m => m.UserId);

            foreach(var movedMember in moveTransaction.MovedMembers)
            {             
                var member = projectMembersById[movedMember];

                var outboxMessage = OutboxMessageModel.Create(
                    new ProjectMemberDeletedMessage
                    {
                        Username = member.Username,
                        UserId = member.UserId,
                        ProjectId = projectId,
                        ProjectTitle = member.ProjectTitle
                    }, Topics.ProjectMembers, MessageActions.Deleted);

                await _projectMembersRepository.DeleteMemberFromProjectAsync(projectId, movedMember, outboxMessage);                
            }

            Console.WriteLine("Rollback completed");
        }
    }
}