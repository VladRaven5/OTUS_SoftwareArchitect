using System.Threading.Tasks;
using Dapper;
using Shared;

namespace WorkingHoursService
{
    public class TasksRepository : BaseDapperRepository
    {
        protected override string _tableName => "tasks";        

        public TasksRepository(PostgresConnectionManager connectionManager)
            : base(connectionManager.GetConnection())
        {
        }

        public Task<TaskModel> GetTaskAsync(string taskId)
        {
            return GetModelByIdAsync<TaskModel>(taskId);
        }

        public async Task CreateOrUpdateTaskAsync(TaskModel task)
        {
            var existingTask = await GetModelByIdAsync<TaskModel>(task.Id);

            if(existingTask == null)
            {
                await CreateTaskAsync(task);
            }
            else
            {
                await UpdateTaskAsync(task);
            }
        }
        
        private async Task CreateTaskAsync(TaskModel newTask)
        {
            string insertQuery = $"insert into {_tableName} (id, projectid, title) "
                + $"values('{newTask.Id}', '{newTask.ProjectId}', '{newTask.Title}');";

            int res = await _connection.ExecuteAsync(insertQuery);

            if(res <= 0)
            {
                throw new DatabaseException("Create task failed");
            }
        }

        private async Task UpdateTaskAsync(TaskModel task)
        {
            string updateQuery = $"update {_tableName} set title = '{task.Title}', projectid = '{task.ProjectId}' where id = '{task.Id}';";

            int res = await _connection.ExecuteAsync(updateQuery);

            if(res <= 0)
            {
                throw new DatabaseException("Update task failed");
            }
        }

        public Task DeleteProjectAsync(string taskId)
        {
            return DeleteModelAsync(taskId);
        }
    }
}