using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;

namespace TasksService
{
    public class TasksRepository
    {
        private readonly ConnectionManager _connectionManager;

        public TasksRepository()
        {
            _connectionManager = new ConnectionManager();
        }

        public async Task<IEnumerable<TaskModel>> GetTasks()
        {
            using(var connection = _connectionManager.GetConnection())
            {
                string query = $"select * from tasks;";
                return await connection.QueryAsync<TaskModel>(query);
            }
        }

        public async Task<TaskModel> GetTask(string taskId)
        {
            using(var connection = _connectionManager.GetConnection())
            {
                string query = $"select * from tasks where id = '{taskId}' limit 1;";
                return await connection.QueryFirstOrDefaultAsync<TaskModel>(query);
            }
        }

        public async Task<TaskModel> CreateTask(string title, string assignedTo)
        {
            using(var connection = _connectionManager.GetConnection())
            {
                string taskId = Guid.NewGuid().ToString();
                DateTimeOffset createdDate = DateTimeOffset.UtcNow;
                TaskState initialState = TaskState.Proposed;

                string insertQuery = "insert into tasks (id, title, createddate, assignedto, state) "
                    + $"values('{taskId}', '{title}', '{createdDate}', '{assignedTo}', '{initialState}');";

                int res = await connection.ExecuteAsync(insertQuery);

                if(res <= 0)
                {
                    throw new Exception("Create failed");
                }

                string selectNewItemQuery = $"select * from tasks where id = '{taskId}' limit 1;";

                return await connection.QueryFirstOrDefaultAsync<TaskModel>(selectNewItemQuery);
            }
        }

        public async Task<TaskModel> UpdateTask(TaskModel updatedTask)
        {
            using(var connection = _connectionManager.GetConnection())
            {
                string updateQuery = $"update tasks set title = '{updatedTask.Title}', createddate = '{updatedTask.CreatedDate}', " +
                    $"assignedto = '{updatedTask.AssignedTo}', state = '{updatedTask.State}' where id = '{updatedTask.Id}';";

                int res = await connection.ExecuteAsync(updateQuery);

                if(res <= 0)
                {
                    throw new Exception("Update failed");
                }

                string selectUpdatedItemQuery = $"select * from tasks where id = '{updatedTask.Id}' limit 1;";

                return await connection.QueryFirstOrDefaultAsync<TaskModel>(selectUpdatedItemQuery);
            }
        }

        public async Task<bool> DeleteTask(string taskId)
        {
            using(var connection = _connectionManager.GetConnection())
            {
                string deleteQuery = $"delete from tasks where id = '{taskId}';";
                int res = await connection.ExecuteAsync(deleteQuery);
                return res > 0;
            }
        }

        public async Task<bool> ClearTasks()
        {
            using(var connection = _connectionManager.GetConnection())
            {
                string clearQuery = $"delete from tasks;";
                int res = await connection.ExecuteAsync(clearQuery);
                return res > 0;
            }
        }
    }
}