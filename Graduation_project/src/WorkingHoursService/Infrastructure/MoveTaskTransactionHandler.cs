using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Shared;

namespace WorkingHoursService
{
    public class MoveTaskTransactionHandler : TransactionHandlerBase
    {
        private readonly ProjectsRepository _projectsRepository;
        private readonly WorkingHoursRepository _workingHoursRepository;
        private readonly TasksRepository _tasksRepository;

        public MoveTaskTransactionHandler(ProjectsRepository projectsRepository, WorkingHoursRepository workingHoursRepository, TasksRepository tasksRepository,
            RequestsRepository requestsRepository, TransactionsRepository transactionsRepository) : base(requestsRepository, transactionsRepository)
        {
            _projectsRepository = projectsRepository;
            _workingHoursRepository = workingHoursRepository;
            _tasksRepository = tasksRepository;
        }

        protected override Task<string> HandleMessageInternalAsync(string messageString, string messageAction)
        {
            switch(messageAction)
            {
                case TransactionMessageActions.MoveTask_HandleHoursRequested:
                    return UpdateHoursWithNewProjectAsync(messageString);

                default:
                    throw new Exception($"unknown message action {messageAction}");
            }
        }

        private async Task<string> UpdateHoursWithNewProjectAsync(string messageString)
        {
            Console.WriteLine("Prepare hours stage started");
            MoveTaskHandleHoursMessage message = JsonConvert.DeserializeObject<MoveTaskHandleHoursMessage>(messageString);

            string transactionId = message.TransactionId;
            var transaction = await _transactionsRepository.GetTransactionAsync(transactionId);

            if(transaction != null && transaction.State == TransactionStates.Denied)
            {
                return $"Transaction {transactionId} already denied";
            }

            var moveTaskTransaction = transaction == null
                ? MoveTaskTransaction.Create(transactionId, message.TaskId, message.ProjectId)
                : MoveTaskTransaction.CreateFromBase(transaction);

            moveTaskTransaction.State = TransactionStates.Processing;          

            string projectTitle = "undefined";
            string taskId = message.TaskId;
            var task = await _tasksRepository.GetTaskAsync(taskId);
            if(task != null)   
            {
                string projectId = task.ProjectId;
                var project = await _projectsRepository.GetProjectAsync(projectId);
                projectTitle = project?.Title;
            }

            string postfix = $"(from {projectTitle})";
            moveTaskTransaction.AppliedPostfix = postfix;

            var taskWorkingHours = (await _workingHoursRepository.GetProjectTaskMemberWorkingHoursAsync(taskId: moveTaskTransaction.TaskId)).ToList();

            List<string> affectedRecordsIds = new List<string>();
            foreach(var workingHoursRecord in taskWorkingHours)
            {
                workingHoursRecord.Description += postfix;
                await  _workingHoursRepository.UpdateWorkingHoursRecordAsync(workingHoursRecord);
                affectedRecordsIds.Add(workingHoursRecord.Id);
            }

            moveTaskTransaction.AffectedRecordsIds = affectedRecordsIds;
            moveTaskTransaction.UpdateData();

            var outboxMessage = OutboxMessageModel.Create(
                new MoveTaskHoursHandledMessage
                {
                    TransactionId = moveTaskTransaction.Id,
                }, Topics.WorkingHours, TransactionMessageActions.MoveTask_HandleHoursCompleted);

            await _transactionsRepository.CreateOrUpdateTransactionAsync(moveTaskTransaction, outboxMessage);

            return $"Working hours updated successful";  
        }        

        protected override async Task RollbackTransactionInternalAsync(TransactionBase transaction)
        {
            Console.WriteLine("Rollback started");

            var moveTransaction = MoveTaskTransaction.CreateFromBase(transaction);
            if(moveTransaction.AffectedRecordsIds.IsNullOrEmpty())
            {
                Console.WriteLine("Nothing to rollback");
                return;
            }

            string postfix = moveTransaction.AppliedPostfix;
            int postfixLength = postfix.Length; 

            var taskWorkingHours = (await _workingHoursRepository.GetProjectTaskMemberWorkingHoursAsync(taskId: moveTransaction.TaskId))
                .ToDictionary(r => r.Id);

            foreach(var affectedRecordId in moveTransaction.AffectedRecordsIds)
            {
                if(taskWorkingHours.TryGetValue(affectedRecordId, out var record))
                {
                    if(record.Description.EndsWith(postfix))
                    {
                        record.Description = record.Description.Remove(record.Description.Length - postfixLength, postfixLength);
                        await _workingHoursRepository.UpdateWorkingHoursRecordAsync(record);
                    }
                }            
            }                  

            Console.WriteLine("Rollback completed");
        }
    }
}