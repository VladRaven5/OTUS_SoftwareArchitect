using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Shared;

namespace ProjectMembersService
{
    public class MoveTaskTransaction : TransactionBase
    {
        public static MoveTaskTransaction Create(string transactionId, string projectId)
        {           
            var transaction = new MoveTaskTransaction
            {
                Type = TransactionTypes.MoveTaskTransaction,
                ProjectId = projectId,
                MovedMembers = new List<string>(),
                State = TransactionStates.Pending,
                Message = $"Moving to another project",
            };

            transaction.Init();
            transaction.Id = transactionId;

            transaction.UpdateData();

            return transaction;
        }

        public static MoveTaskTransaction CreateFromBase(TransactionBase baseTransaction)
        {
            if(baseTransaction.Type != TransactionTypes.MoveTaskTransaction)
            {
                throw new Exception($"Transaction type is {baseTransaction.Type}, but expected {TransactionTypes.MoveTaskTransaction}");
            }

            var data = JsonConvert.DeserializeObject<MoveTaskTransactionData>(baseTransaction.Data);

            return new MoveTaskTransaction
            {
                Id = baseTransaction.Id,
                Type = baseTransaction.Type,
                CreatedDate = baseTransaction.CreatedDate,
                ObjectId = baseTransaction.ObjectId,
                Data = baseTransaction.Data,
                Message = baseTransaction.Message, 
                State = baseTransaction.State,

                ProjectId = data.ProjectId,
                MovedMembers = data.MovedMembers
            };
        }

        public string ProjectId { get; set; }
        public IEnumerable<string> MovedMembers  { get; set; }

        public void UpdateData()
        {
            var transactionData = new MoveTaskTransactionData
            {
                ProjectId = ProjectId,
                MovedMembers = MovedMembers
            };

            Data = JsonConvert.SerializeObject(transactionData);
        }

        private class MoveTaskTransactionData
        {
            public string ProjectId { get; set; }
            public IEnumerable<string> MovedMembers  { get; set; }
        }
    }    
}