using System;
using System.Collections.Generic;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Shared;

namespace TasksService
{
    public class TransactionMessagesHandler : DomainMessagesHandlerBase<TransactionsRepository>
    {
        public static readonly HashSet<string> Actions = new HashSet<string>
        {
            TransactionMessageActions.MoveTask_PrepareListCompleted,
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

                        //COMPLETE FROM HERE
                        moveTaskTransaction.State = TransactionStates.Success;                        

                        var task = tasksRepository.GetSimpleTaskAsync(moveTaskTransaction.ObjectId).GetAwaiter().GetResult();

                        var projectsRepository = scope.ServiceProvider.GetRequiredService<ProjectsRepository>();
                        var project = projectsRepository.GetProjectAsync(moveTaskTransaction.ProjectId).GetAwaiter().GetResult();

                        var listsRepository = scope.ServiceProvider.GetRequiredService<ListsRepository>();                      
                        var list = listsRepository.GetListAsync(newListId).GetAwaiter().GetResult();                        

                        task.ListId = newListId;

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

                        var transactionOutboxMessage = OutboxMessageModel.Create(
                            new CompleteTransactionMessage
                            {
                                TransactionId = transactionId
                            }, Topics.Tasks, TransactionMessageActions.MoveTask_Complete);
                        
                        repository.CreateOrUpdateTransactionAsync(moveTaskTransaction, transactionOutboxMessage).GetAwaiter().GetResult();
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
    }
}