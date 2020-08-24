using System;
using Newtonsoft.Json;
using Shared;

namespace ListsService
{
    public class MoveTaskTransaction : TransactionBase
    {
        public static MoveTaskTransaction Create(string transactionId, string projectId, string listTitle)
        {           
            var transaction = new MoveTaskTransaction
            {
                Type = TransactionTypes.MoveTaskTransaction,
                ProjectId = projectId,
                ListTitle = listTitle,
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
                ListTitle = data.ListTitle,
                ListId = data.ListId,
                IsListCreated = data.IsListCreated
            };
        }

        public string ListId { get; set; }
        public string ProjectId { get; set; }
        public string ListTitle { get; set; }
        public bool IsListCreated { get; set; }

        public void UpdateData()
        {
            var transactionData = new MoveTaskTransactionData
            {
                ProjectId = ProjectId,
                ListTitle = ListTitle,
                ListId = ListId,
                IsListCreated = IsListCreated
            };


            Data = JsonConvert.SerializeObject(transactionData);
        }

        private class MoveTaskTransactionData
        {
            public string ProjectId { get; set; }
            public string ListTitle { get; set; }
            public string ListId { get; set; }
            public bool IsListCreated { get; set; }
        }
    }

}