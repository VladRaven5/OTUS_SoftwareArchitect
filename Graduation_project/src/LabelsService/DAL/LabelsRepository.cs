using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Shared;

namespace LabelsService
{
    public class LabelsRepository : BaseDapperRepository
    {
        protected override string _tableName => "labels";

        public LabelsRepository(PostgresConnectionManager connectionManager)
            : base(connectionManager.GetConnection())
        {
        }

        public Task<IEnumerable<LabelModel>> GetLabelsAsync()
        {
            return GetModelsAsync<LabelModel>();
        }

        public Task<LabelModel> GetLabelByIdAsync(string labelId)
        {
            return GetModelByIdAsync<LabelModel>(labelId);
        }

        public async Task<LabelModel> CreateLabelAsync(LabelModel newLabel, OutboxMessageModel message)
        {
            string insertQuery = $"insert into {_tableName} (id, title, color, createddate, version) "
                + $"values('{newLabel.Id}', '{newLabel.Title}', '{newLabel.Color}', '{newLabel.CreatedDate}', {newLabel.Version});";

            string insertMessageQuery = ConstructInsertMessageQuery(message);

            insertQuery += insertMessageQuery;

            int res = await _connection.ExecuteAsync(insertQuery);

            if(res <= 0)
            {
                throw new DatabaseException("Create label failed");
            }            

            return await GetLabelByIdAsync(newLabel.Id);
        }

        public async Task<LabelModel> UpdateLabelAsync(LabelModel updatedLabel, OutboxMessageModel message)
        {
            int newVersion = updatedLabel.Version + 1;

            string updateQuery = $"update {_tableName} set " + 
                $"title = '{updatedLabel.Title}', " +
                $"color = '{updatedLabel.Color}', " +            
                $"version = {newVersion} " +
                $"where id = '{updatedLabel.Id}';";

            string insertMessageQuery = ConstructInsertMessageQuery(message);
            updateQuery += insertMessageQuery;

            int res = await _connection.ExecuteAsync(updateQuery);

            if(res <= 0)
            {
                throw new DatabaseException("Update label failed");
            }

            return await GetLabelByIdAsync(updatedLabel.Id);
        }

        public Task DeleteLabelAsync(string labelId, OutboxMessageModel message)
        {
            return DeleteModelAsync(labelId, message);          
        }
    }
}