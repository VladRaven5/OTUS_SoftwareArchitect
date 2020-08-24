using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Shared
{
    public abstract class TransactionHandlerBase
    {
        public static readonly HashSet<string> Actions = new HashSet<string>
        {
            TransactionMessageActions.MoveTask_PrepareListRequested,
            TransactionMessageActions.MoveTask_PrepareListCompleted,
            TransactionMessageActions.MoveTask_MoveMembersRequested,
            TransactionMessageActions.MoveTask_MoveMembersCompleted,
            TransactionMessageActions.MoveTask_HandleHoursRequested,
            TransactionMessageActions.MoveTask_HandleHoursCompleted,
            TransactionMessageActions.MoveTask_Rollback,
            TransactionMessageActions.MoveTask_Complete,
        };

        private readonly RequestsRepository _requestsRepository;
        protected readonly TransactionsRepository _transactionsRepository;

        protected TransactionHandlerBase(RequestsRepository requestsRepository, TransactionsRepository transactionsRepository)
        {
            _requestsRepository = requestsRepository;
            _transactionsRepository = transactionsRepository;
        }

        public async Task<string> HandleMessageAsync(ReceivedMessageArgs messageObject)
        {
            var baseMessage = JsonConvert.DeserializeObject<BaseTransactionMessage>(messageObject.Message);

            if(await _requestsRepository.IsHandledOrSaveRequestAsync(baseMessage.Id, GetRequestIdInvalidationDate()))
            {
                return $"Already handled: {baseMessage.Id}";
            }

            try
            {
                switch(messageObject.Action)
                {                    
                    case TransactionMessageActions.MoveTask_Complete:
                        return await CompleteTransactionAsync(messageObject.Message);
                    case TransactionMessageActions.MoveTask_Rollback:
                        return await RollbackTransactionAsync(messageObject.Message);

                    default:
                        return await HandleMessageInternalAsync(messageObject.Message, messageObject.Action);
                }     
            }
            catch
            {
                await _requestsRepository.DeleteRequestIdAsync(baseMessage.Id);
                throw;
            }                   
        }        

        protected abstract Task<string> HandleMessageInternalAsync(string messageString, string messageAction);

        private async Task<string> CompleteTransactionAsync(string messageString)
        {
            Console.WriteLine("Complete transaction");
            CompleteTransactionMessage message = JsonConvert.DeserializeObject<CompleteTransactionMessage>(messageString);

            string transactionId = message.TransactionId;
            var transaction = await _transactionsRepository.GetTransactionAsync(transactionId);

            if(transaction != null && transaction.State == TransactionStates.Denied)
            {
                return $"Transaction {transactionId} already denied";
            }

            if(transaction == null)
            {
                transaction = new TransactionBase();
                transaction.Init();
                transaction.Id = message.TransactionId;
            }

            transaction.State = TransactionStates.Success;
            transaction.Message = "Completed";
            await _transactionsRepository.CreateOrUpdateTransactionAsync(transaction, null);

            return $"Transaction {transactionId} completed";
        }

        private async Task<string> RollbackTransactionAsync(string messageString)
        {
            Console.WriteLine("Deny transaction");
            RollbackTransactionMessage message = JsonConvert.DeserializeObject<RollbackTransactionMessage>(messageString);

            string transactionId = message.TransactionId;
            var transaction = await _transactionsRepository.GetTransactionAsync(transactionId);

            if(transaction != null && transaction.State == TransactionStates.Denied)
            {
                return $"Transaction {transactionId} already denied";
            }

            if(transaction == null)
            {
                transaction = new TransactionBase();
                transaction.Init();
                transaction.Id = message.TransactionId;
            }
            else
            {
                //TODO: Rework for different transaction handlers
                await RollbackTransactionInternalAsync(transaction);

            }

            transaction.State = TransactionStates.Denied;
            transaction.Message = message.Reason;
            await _transactionsRepository.CreateOrUpdateTransactionAsync(transaction, null);

            return $"Transaction {transactionId} denied";
        }

        protected abstract Task RollbackTransactionInternalAsync(TransactionBase transaction);

        protected virtual DateTimeOffset GetRequestIdInvalidationDate()
        {
            return DateTimeOffset.UtcNow.AddDays(Constants.BrokerMessageLifetimeDays);
        }
    }    
} 