using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Shared;

namespace TasksService
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

        public Task<IEnumerable<LabelModel>> GetLabelsAsync(IEnumerable<string> labelsIds)
        {
            return GetModelsWithCheckIsAllAsync<LabelModel>(labelsIds);
        }

        public Task<LabelModel> GetLabelAsync(string labelId)
        {
            return GetModelByIdAsync<LabelModel>(labelId);
        }

        public async Task CreateOrUpdateLabelAsync(LabelModel label)
        {
            var existingLabel = await GetModelByIdAsync<LabelModel>(label.Id);

            if(existingLabel == null)
            {
                await CreateLabelAsync(label);
            }
            else
            {
                await UpdateLabelAsync(label);
            }
        }
        
        private async Task CreateLabelAsync(LabelModel newLabel)
        {
            string insertQuery = $"insert into {_tableName} (id, title, color) "
                + $"values('{newLabel.Id}', '{newLabel.Title}', '{newLabel.Color}');";

            int res = await _connection.ExecuteAsync(insertQuery);

            if(res <= 0)
            {
                throw new DatabaseException("Create label failed");
            }
        }

        private async Task UpdateLabelAsync(LabelModel label)
        {
            string updateQuery = $"update {_tableName} set title = '{label.Title}', color='{label.Color}' where id = '{label.Id}';";

            int res = await _connection.ExecuteAsync(updateQuery);

            if(res <= 0)
            {
                throw new DatabaseException("Update label failed");
            }
        }

        public Task DeleteLabelAsync(string labelId)
        {
            return DeleteModelAsync(labelId);
        }
    }
}