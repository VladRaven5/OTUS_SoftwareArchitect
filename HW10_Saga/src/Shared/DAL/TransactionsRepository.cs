using System.Threading.Tasks;
using Dapper;

namespace Shared
{
    public class TransactionsRepository : BaseDapperRepository
    {
        protected override string _tableName => "transactions";

        public TransactionsRepository(PostgresConnectionManager connectionManager)
            : base(connectionManager.GetConnection())
        {
        }

        public Task<TransactionBase> GetTransactionAsync(string id)
        {
            return GetModelByIdAsync<TransactionBase>(id);
        }

        public async Task<TransactionBase> CreateTransactionRecordAsync(TransactionBase transactionRecord)
        {
            string query = $"insert into {_tableName} (id, objectid, type, data, message, state, createddate) " + 
                $" values ('{transactionRecord.Id}', '{transactionRecord.ObjectId}', '{transactionRecord.Type}', '{transactionRecord.Data}', '{transactionRecord.Message}', {(int)transactionRecord.State}, '{transactionRecord.CreatedDate}');";
            
            var result = await _connection.ExecuteAsync(query);

            if(result <= 0)
            {
                throw new DatabaseException("Transaction insertion failed");
            }

            return await GetTransactionAsync(transactionRecord.Id);
        }

        public async Task<TransactionBase> UpdateTransactionRecordAsync(string id, string data, string message, TransactionState? state)
        {
            string AddStatement(string targetStatements, string addingStatement)
            {
                if(string.IsNullOrWhiteSpace(targetStatements))
                    return addingStatement;

                return targetStatements + ", " + addingStatement;
            }

            string statements = string.Empty;

            if(!string.IsNullOrWhiteSpace(data))
            {
                statements = AddStatement(statements, $" data = '{data}' ");
            }

            if(!string.IsNullOrWhiteSpace(message))
            {
                statements = AddStatement(statements, $" message = '{message}' ");
            }

            if(state.HasValue)
            {
                statements = AddStatement(statements, $" state = {(int)state.Value} ");
            }

            if(string.IsNullOrWhiteSpace(statements))
                return null;        


            string query = $"update {_tableName} set {statements} where id = {id} ";
            var result = await _connection.ExecuteAsync(query);

            if(result <= 0)
            {
                throw new DatabaseException("Transaction updation failed");
            }

            return await GetTransactionAsync(id);
        }         
    }
}