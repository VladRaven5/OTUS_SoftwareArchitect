using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Shared;

namespace ListsService
{
    public class MoveTaskTransactionHandler : TransactionHandlerBase
    {
        private readonly ListsRepository _listsRepository;
        private readonly ProjectsRepository _projectsRepository;

        public MoveTaskTransactionHandler(ListsRepository listsRepository, ProjectsRepository projectsRepository,
            TransactionsRepository transactionsRepository, RequestsRepository requestsRepository, RabbitMqTopicManager rabbit)
            : base(requestsRepository, transactionsRepository)
        {
            _listsRepository = listsRepository;
            _projectsRepository = projectsRepository;
        }

        protected override Task<string> HandleMessageInternalAsync(string messageString, string messageAction)
        {
            switch(messageAction)
            {
                case TransactionMessageActions.MoveTask_PrepareListRequested:
                    return PrepareListStageAsync(messageString);

                default:
                    throw new Exception($"unknown message action {messageAction}");
            }
        }        

        private async Task<string> PrepareListStageAsync(string messageString)
        {
            Console.WriteLine("Prepare list stage started");
            MoveTaskPrepareListMessage message = JsonConvert.DeserializeObject<MoveTaskPrepareListMessage>(messageString);
            return await PrepareListStageInternalAsync(message);
        }        

        private async Task<string> PrepareListStageInternalAsync(MoveTaskPrepareListMessage message)
        {
            string transactionId = message.TransactionId;
            var transaction = await _transactionsRepository.GetTransactionAsync(transactionId);

            if(transaction != null && transaction.State == TransactionStates.Denied)
            {
                return $"Transaction {transactionId} already denied";
            }

            var moveTransaction = transaction == null
                ? MoveTaskTransaction.Create(transactionId, message.ProjectId, message.ListTitle)
                : MoveTaskTransaction.CreateFromBase(transaction);

            moveTransaction.State = TransactionStates.Processing;

            string projectId = message.ProjectId;
            var project = await _projectsRepository.GetProjectAsync(projectId);
            if(project == null)
            {
                string reason = $"Project {projectId} not found";                

                var rollbackOutboxMessage = OutboxMessageModel.Create(
                    new RollbackTransactionMessage
                    {
                        TransactionId = transactionId,
                        Reason = reason
                    }, Topics.Lists, TransactionMessageActions.MoveTask_Rollback);

                moveTransaction.State = TransactionStates.Denied;
                moveTransaction.Message = reason;
                moveTransaction.UpdateData();                   

                await _transactionsRepository.CreateOrUpdateTransactionAsync(moveTransaction, rollbackOutboxMessage);

                return reason;
            }

            try
            {
                var projectLists = (await _listsRepository.GetProjectListsAsyn(projectId)).ToList();
                var sameTitleList = projectLists.FirstOrDefault(list => list.Title == message.ListTitle);                

                if(sameTitleList == null)
                {
                    var newList = new ListModel
                    {
                        Title = message.ListTitle,
                        ProjectId = projectId
                    };

                    newList.Init();

                    moveTransaction.ListId = newList.Id;
                    moveTransaction.IsListCreated = true;                

                    var completedOutboxMessage = OutboxMessageModel.Create(
                        new ListCreatedMessage
                        {
                            ListId = newList.Id,
                            ProjectId = newList.ProjectId,
                            Title = newList.Title
                        }, Topics.Lists, MessageActions.Created);

                    await _listsRepository.CreateListAsync(newList, completedOutboxMessage);
                }
                else
                {
                    moveTransaction.ListId = sameTitleList.Id;
                    moveTransaction.IsListCreated = false;
                }

                moveTransaction.UpdateData();

                var outboxMessage = OutboxMessageModel.Create(
                    new MoveTaskListPreparedMessage
                    {
                        TransactionId = transactionId,
                        ListId = moveTransaction.ListId
                    }, Topics.Lists, TransactionMessageActions.MoveTask_PrepareListCompleted);

                await _transactionsRepository.CreateOrUpdateTransactionAsync(moveTransaction, outboxMessage);

                return "Prepare list stage completed successful";
            }
            catch(Exception e)
            {
                string errorReason = $"Unknonw exception occured:\n{e.Message}\n{e.StackTrace}";
                moveTransaction.Message = errorReason;
                moveTransaction.State = TransactionStates.Denied;

                var failedOutboxMessage = OutboxMessageModel.Create(
                    new RollbackTransactionMessage
                    {
                        TransactionId = transactionId,
                        Reason = moveTransaction.Message
                    }, Topics.Lists, TransactionMessageActions.MoveTask_Rollback);

                await _transactionsRepository.CreateOrUpdateTransactionAsync(moveTransaction, failedOutboxMessage);

                throw;
            }
        } 

        protected override Task RollbackTransactionInternalAsync(TransactionBase transaction)
        {
            var moveTransaction = MoveTaskTransaction.CreateFromBase(transaction);

            if(!moveTransaction.IsListCreated || moveTransaction.ListId == null)
                return Task.CompletedTask; //nothing to do here

            var outboxMessage = OutboxMessageModel.Create(
                new ListDeletedMessage
                {
                    ListId = moveTransaction.ListId,
                }, Topics.Lists, MessageActions.Deleted);

            return _listsRepository.DeleteListAsync(moveTransaction.ListId, outboxMessage);                       
        }       
    }    
}