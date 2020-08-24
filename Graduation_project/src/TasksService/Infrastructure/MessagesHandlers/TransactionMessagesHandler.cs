using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Shared;

namespace TasksService
{
    public class TransactionMessagesHandler : DomainMessagesHandlerBase<TransactionsRepository>
    {
        public static readonly HashSet<string> Actions = new HashSet<string>
        {
            TransactionMessageActions.MoveTask_PrepareListCompleted,
            TransactionMessageActions.MoveTask_MoveMembersCompleted,
            TransactionMessageActions.MoveTask_HandleHoursCompleted,
            TransactionMessageActions.MoveTask_Rollback,
            TransactionMessageActions.MoveTask_Complete,
        };

        protected override string _identifier => "Transactions";

        public TransactionMessagesHandler(IServiceProvider serviceProvider, IMapper mapper)
            : base(serviceProvider, mapper)
        {            
        }

        protected override Type GetMessageTypeForAction(string action)
        {
            switch(action)
            {
                case TransactionMessageActions.MoveTask_PrepareListCompleted:
                    return typeof(MoveTaskListPreparedMessage);

                case TransactionMessageActions.MoveTask_MoveMembersCompleted:
                    return typeof(MoveTaskMembersMovedMessage);

                case TransactionMessageActions.MoveTask_HandleHoursCompleted:
                    return typeof(MoveTaskHoursHandledMessage);
                
                case TransactionMessageActions.MoveTask_Rollback:
                    return typeof(RollbackTransactionMessage);

                case TransactionMessageActions.MoveTask_Complete:
                    return typeof(CompleteTransactionMessage);
                
                default:
                    throw new Exception($"Unknown action {action} in {_identifier}");
            }
        }

        protected override void HandleMessageInternal(BaseMessage message, string action, TransactionsRepository repository)
        {
            Console.WriteLine($"Handle transaction message");
            var transactionMessage = message as BaseTransactionMessage;
            string transactionId = transactionMessage.TransactionId;
            var transaction = repository.GetTransactionAsync(transactionId).GetAwaiter().GetResult();
            if(transaction?.State == TransactionStates.Denied)
            {
                Console.WriteLine($"Transaction {transaction?.Id} already denied");
                return;
            }

            using(var scope = _serviceProvider.CreateScope())
            {
                var tasksRepository = scope.ServiceProvider.GetRequiredService<TasksRepository>();

                switch(action)
                {
                    case TransactionMessageActions.MoveTask_PrepareListCompleted:
                        HandleListPreparedAsync(transaction, transactionMessage, repository, tasksRepository).GetAwaiter().GetResult();                        
                        break;


                    case TransactionMessageActions.MoveTask_MoveMembersCompleted:
                        HandleMembersMovedAsync(transaction, transactionMessage, repository).GetAwaiter().GetResult();
                        break;


                    case TransactionMessageActions.MoveTask_HandleHoursCompleted:
                        var hoursHanldedMessage = message as MoveTaskHoursHandledMessage;
                        if(transaction == null)
                        {
                            Console.WriteLine($"Transaction for handled hours not found");
                            return;
                        }
                        var moveTaskHoursTransaction = MoveTaskTransaction.CreateFromBase(transaction);             

                        moveTaskHoursTransaction.State = TransactionStates.Success; 

                        string resultListId = moveTaskHoursTransaction.ListId;                       

                        var task = tasksRepository.GetSimpleTaskAsync(moveTaskHoursTransaction.ObjectId).GetAwaiter().GetResult();

                        var projectsRepository = scope.ServiceProvider.GetRequiredService<ProjectsRepository>();
                        var project = projectsRepository.GetProjectAsync(moveTaskHoursTransaction.ProjectId).GetAwaiter().GetResult();

                        var listsRepository = scope.ServiceProvider.GetRequiredService<ListsRepository>();                      
                        var list = listsRepository.GetListAsync(resultListId).GetAwaiter().GetResult();                        

                        task.ListId = resultListId;

                        var taskNewListOutboxMessage = OutboxMessageModel.Create(
                            new TaskUpdatedMessage
                            {
                                TaskId = task.Id,
                                Title = task.Title,
                                ProjectId = project.Id,
                                ProjectTitle = project.Title,
                                ListId = list.Id,
                                ListTitle = list.Title                       
                            }, Topics.Tasks, MessageActions.Updated);

                        tasksRepository.UpdateTaskAsync(task, taskNewListOutboxMessage).GetAwaiter().GetResult();
                        tasksRepository.UnsetTransactionAsync(task.Id, transactionId).GetAwaiter().GetResult();

                        var taskMembers = tasksRepository.GetTaskMembersIdsAsync(task.Id)
                            .GetAwaiter()
                            .GetResult()
                            ?.ToList();                        

                        var transactionCompletedOutboxMessage = OutboxMessageModel.Create(
                            new CompleteTransactionMessage
                            {
                                TransactionId = transactionId
                            }, Topics.Tasks, TransactionMessageActions.MoveTask_Complete);
                        
                        repository.CreateOrUpdateTransactionAsync(moveTaskHoursTransaction, transactionCompletedOutboxMessage).GetAwaiter().GetResult();

                        Console.WriteLine($"Complete transaction {transactionId}");

                        //invalidate caches if any member
                        if(!taskMembers.IsNullOrEmpty())
                        {
                            var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();                            
                            List<Task> cacheInvalidationTasks = new List<Task>();

                            string taskCacheKey = string.Format(CacheSettings.TaskIdCacheKeyPattern, task.Id);
                            cacheInvalidationTasks.Add(cache.RemoveAsync(taskCacheKey));

                            foreach(var user in taskMembers)
                            {
                                string userCacheKey = string.Format(CacheSettings.UserTasksCacheKeyPattern, user);
                                cacheInvalidationTasks.Add(cache.RemoveAsync(userCacheKey));
                            }

                            Task.WhenAll(cacheInvalidationTasks).GetAwaiter().GetResult();
                            Console.WriteLine($"Cache invalidated");
                        }

                        break;




                    case TransactionMessageActions.MoveTask_Rollback:
                        if(string.IsNullOrWhiteSpace(transaction?.ObjectId))
                        {
                            Console.WriteLine("Rollback with no data changed");
                            return;
                        }

                        var rollbackMessage = message as RollbackTransactionMessage;

                        tasksRepository.UnsetTransactionAsync(transaction.ObjectId, transactionId).GetAwaiter().GetResult();
                        transaction.State = TransactionStates.Denied;
                        transaction.Message = rollbackMessage.Reason;

                        var rollbackOutboxMessage = OutboxMessageModel.Create(
                            new RollbackTransactionMessage
                            {
                                TransactionId = transactionId,
                                Reason = rollbackMessage.Reason
                            }, Topics.Tasks, TransactionMessageActions.MoveTask_Rollback);

                        repository.CreateOrUpdateTransactionAsync(transaction, rollbackOutboxMessage).GetAwaiter().GetResult();            
                        break;
                }
            }            
        }        

        private async Task HandleListPreparedAsync(TransactionBase transaction, BaseTransactionMessage message, TransactionsRepository repository,
            TasksRepository tasksRepository)
        {
            var listPreparedMessage = message as MoveTaskListPreparedMessage;
            if(transaction == null)
            {
                Console.WriteLine($"Transaction for prepared list {listPreparedMessage.ListId} not found");
                return;
            }

            var moveTaskTransaction = MoveTaskTransaction.CreateFromBase(transaction);

            string newListId = listPreparedMessage.ListId;

            moveTaskTransaction.ListId = newListId;
            moveTaskTransaction.IsListPrepared = true;
            moveTaskTransaction.UpdateData();

            string taskId = moveTaskTransaction.ObjectId;

            IEnumerable<string> taskMembersIds = await tasksRepository.GetTaskMembersIdsAsync(taskId);
            OutboxMessageModel nextMessage = default(OutboxMessageModel);
            
            if(!taskMembersIds.IsNullOrEmpty())
            {
                var task = await tasksRepository.GetTaskAsync(taskId);

                moveTaskTransaction.Message = "Move members to project";

                nextMessage = OutboxMessageModel.Create(
                    new MoveTaskMoveMembersMessage()
                    {
                        TransactionId = transaction.Id,
                        TargetProjectId = moveTaskTransaction.ProjectId,
                        SourceProjectId = task.ProjectId,
                        TaskMembers = taskMembersIds
                    }, Topics.Tasks, TransactionMessageActions.MoveTask_MoveMembersRequested);
            }
            else
            {
                moveTaskTransaction.AreMembersPrepared = true;
                moveTaskTransaction.Message = "Handle task hours with new project";

                nextMessage = OutboxMessageModel.Create(
                    new MoveTaskHandleHoursMessage()
                    {
                        TransactionId = transaction.Id,
                        ProjectId = moveTaskTransaction.ProjectId,
                        TaskId = taskId
                    }, Topics.Tasks, TransactionMessageActions.MoveTask_HandleHoursRequested);
            }

            await repository.CreateOrUpdateTransactionAsync(moveTaskTransaction, nextMessage);
        }

        private Task HandleMembersMovedAsync(TransactionBase transaction, BaseTransactionMessage message, TransactionsRepository repository)
        {
            var membersMovedMesasge = message as MoveTaskMembersMovedMessage;
            if(transaction == null)
            {
                Console.WriteLine($"Transaction for moved task members not found");
                return Task.CompletedTask;
            }

            var moveTaskTransaction = MoveTaskTransaction.CreateFromBase(transaction);
            moveTaskTransaction.AreMembersPrepared = true;
            moveTaskTransaction.Message = "Handle task hours with new project";

            var outboxMessage = OutboxMessageModel.Create(
                new MoveTaskHandleHoursMessage()
                {
                    TransactionId = transaction.Id,
                    ProjectId = moveTaskTransaction.ProjectId,
                    TaskId = moveTaskTransaction.ObjectId
                }, Topics.Tasks, TransactionMessageActions.MoveTask_HandleHoursRequested);

            return repository.CreateOrUpdateTransactionAsync(moveTaskTransaction, outboxMessage);
        }
    }
}