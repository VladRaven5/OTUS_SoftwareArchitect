using System;
using Newtonsoft.Json;
using Shared;

namespace TasksService
{
    public class MoveTaskTransaction : TransactionBase
    {
        public static MoveTaskTransaction Create(string taskId, string projectId, string listTitle)
        {           
            var transaction = new MoveTaskTransaction
            {
                Type = TransactionTypes.MoveTaskTransaction,
                ObjectId = taskId,
                ProjectId = projectId,
                ListTitle = listTitle,
                State = TransactionStates.Pending,
                Message = $"Moving to another project",
            };

            transaction.Init();
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
                IsListPrepared = data.IsListPrepared,
                AreMembersPrepared = data.AreMembersPrepared
            };
        }

        public string ProjectId { get; set; }
        public string ListTitle { get; set; }
        public string ListId { get; set; }

        public bool IsListPrepared { get; set; }
        public bool AreMembersPrepared { get; set; }

        public void UpdateData()
        {
            var transactionData = new MoveTaskTransactionData
            {
                ProjectId = ProjectId,
                ListTitle = ListTitle,
                ListId = ListId,
                IsListPrepared = IsListPrepared,
                AreMembersPrepared = AreMembersPrepared,
            };


            Data = JsonConvert.SerializeObject(transactionData);
        }

        private class MoveTaskTransactionData
        {
            public string ProjectId { get; set; }
            public string ListTitle { get; set; }
            public string ListId { get; set; }
            public bool IsListPrepared { get; set; }
            public bool AreMembersPrepared { get; set; }
        }
    }     
}