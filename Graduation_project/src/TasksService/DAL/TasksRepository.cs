using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using Shared;

namespace TasksService
{
    public class TasksRepository : BaseDapperRepository
    {
        private readonly IMapper _mapper;

        protected override string _tableName => "tasks";
        private const string _membersJoinTableName = "task_members"; 
        private const string _labelsJoinTableName = "task_labels"; 
        
        public TasksRepository(PostgresConnectionManager connectionManager, IMapper mapper)
            : base(connectionManager.GetConnection())
        {
            _mapper = mapper;
        }

        public async Task<TaskAggregate> GetTaskAsync(string taskId)
        {
            string query = GetMergedSelectQuery($"where t.id = '{taskId}'");
            var queryResults = await _connection.QueryAsync<TaskQueryJoinedResult>(query);
            var aggregatedResult = AggregateQueryResult(queryResults.ToList());

            return aggregatedResult.SingleOrDefault();
        }

        public async Task<IEnumerable<TaskAggregate>> GetAllTasksAsync()
        {
            string query = GetMergedSelectQuery("");
            var queryResults = await _connection.QueryAsync<TaskQueryJoinedResult>(query);
            var aggregatedResult = AggregateQueryResult(queryResults.ToList());

            return aggregatedResult;
        }     

        // public Task<IEnumerable<ProjectMemberAggregate>> GetProjectsMembersAsync(string projectId = null, string userId = null)
        // {
        //     string AddStatement(string target, string statement)
        //     {
        //         if(string.IsNullOrWhiteSpace(target))
        //         {
        //             target = $"where {statement}";
        //         }
        //         else
        //         {
        //             target += $" and {statement}";
        //         }

        //         return target;
        //     }

        //     string whereStatement = string.Empty;

        //     if(!string.IsNullOrWhiteSpace(projectId))
        //     {
        //         whereStatement = AddStatement(whereStatement, $"projectid = '{projectId}'");
        //     }

        //     if(!string.IsNullOrWhiteSpace(userId))
        //     {
        //         whereStatement = AddStatement(whereStatement, $"userId = '{userId}'");
        //     }

        //     return _connection.QueryAsync<ProjectMemberAggregate>(
        //         GetMergedSelectQuery(whereStatement));
        // }

        public async Task<TaskAggregate> CreateTaskAsync(TaskModel newTask, TaskCollections collections, OutboxMessageModel outboxMessage)
        {
            string insertQuery = $"insert into {_tableName} (id, title, description, listid, state, duedate, version, createddate) " +
                $"values('{newTask.Id}', '{newTask.Title}', '{newTask.Description}', '{newTask.ListId}', {(int)newTask.State}, " +
                $"{GetQueryNullableEscapedValue(newTask.DueDate)}, {newTask.Version}, {GetQueryNullableEscapedValue(newTask.CreatedDate)});";

            Task<string> insertTaskMembersQueryTask = ConstructQueryToAddTaskMembersAsync(newTask.Id, collections.Members);
            await insertTaskMembersQueryTask;

            Task<string> insertTaskLabelsQueryTask = ConstructQueryToAddTaskLabelsAsync(newTask.Id, collections.Labels);
            await insertTaskLabelsQueryTask;

            //await Task.WhenAll(insertTaskMembersQueryTask, insertTaskLabelsQueryTask);

            insertTaskMembersQueryTask.CheckAndThrowIfFaulted();
            insertTaskLabelsQueryTask.CheckAndThrowIfFaulted();
            string insertOutboxMessageQuery = ConstructInsertMessageQuery(outboxMessage);

            insertQuery += insertTaskMembersQueryTask.Result;
            insertQuery += insertTaskLabelsQueryTask.Result;
            insertQuery += insertOutboxMessageQuery;

            int res = await _connection.ExecuteAsync(insertQuery);

            if(res <= 0)
            {
                throw new DatabaseException("Add task failed");
            }

            return await GetTaskAsync(newTask.Id);          
        }

        public async Task<TaskAggregate>  UpdateTaskAsync(TaskModel updatingTask, TaskCollections addingCollections,
            TaskCollections removingCollections, OutboxMessageModel outboxMessage)
        {
            string taskId = updatingTask.Id;
            
            int version = updatingTask.Version + 1;

            var updateQuery = $"update {_tableName} set " +
                $"title = '{updatingTask.Title}', " +
                $"description = '{updatingTask.Description}', " +
                $"listid = '{updatingTask.ListId}', " +
                $"state = {(int)updatingTask.State}, " +
                $"duedate = {GetQueryNullableEscapedValue(updatingTask.DueDate)} " +
                $"where id = '{taskId}'; ";

            string deleteOldTaskMembersQuery = ConstructQueryToDeleteTaskMembers(taskId, removingCollections.Members);
            string deleteOldTaskLabelsQuery = ConstructQueryToDeleteTaskLabels(taskId, removingCollections.Labels);

            Task<string> addNewTaskMembersQueryTask = ConstructQueryToAddTaskMembersAsync(taskId, addingCollections.Members);
            await addNewTaskMembersQueryTask;

            Task<string> addNewTaskLabelsQueryTask = ConstructQueryToAddTaskLabelsAsync(taskId, addingCollections.Labels);
            await addNewTaskLabelsQueryTask;

            //await Task.WhenAll(addNewTaskMembersQueryTask, addNewTaskLabelsQueryTask);

            addNewTaskMembersQueryTask.CheckAndThrowIfFaulted();
            addNewTaskLabelsQueryTask.CheckAndThrowIfFaulted();

            string insertOutboxMessageQuery = ConstructInsertMessageQuery(outboxMessage);

            updateQuery += deleteOldTaskMembersQuery;
            updateQuery += deleteOldTaskLabelsQuery;
            updateQuery += addNewTaskMembersQueryTask.Result;
            updateQuery += addNewTaskLabelsQueryTask.Result;
            updateQuery += insertOutboxMessageQuery;

            int res = await _connection.ExecuteAsync(updateQuery);

            if(res <= 0)
            {
                throw new DatabaseException("Update task failed");
            }

            return await GetTaskAsync(updatingTask.Id);
        }

        public Task<TaskModel> GetSimpleTaskAsync(string taskId)
        {
            string query = $"select * from {_tableName} where id = '{taskId}' limit 1 ;";
            return _connection.QueryFirstOrDefaultAsync<TaskModel>(query);
        }

        public Task DeleteTaskAsync(string taskId, OutboxMessageModel outboxMessage)
        {
            string deleteTaskQuery = $"delete from {_tableName} where id = '{taskId}' ; ";
            string deleteTaskMembersQuery = ConstructQueryToDeleteTaskJoinData(_membersJoinTableName, taskId);
            string deleteTaskLabelsQuery = ConstructQueryToDeleteTaskJoinData(_labelsJoinTableName, taskId);
            string insertMessageQuery = ConstructInsertMessageQuery(outboxMessage);

            string resultDeleteQuery = deleteTaskMembersQuery +
                deleteTaskLabelsQuery +
                deleteTaskQuery +
                insertMessageQuery;

            return _connection.ExecuteAsync(resultDeleteQuery);
        }

        private string GetMergedSelectQuery(string whereStatement)
        {
            return "select t.id, p.id as projectid, p.title as projecttitle, " +
                "t.listid, ls.title as listtitle, t.title, t.description, t.state, t.duedate, " +
                "u.id as userid, u.username, lb.id as labelid, lb.title as labeltitle, lb.color as labelcolor, " +
                "t.version, t.createddate as createddate " +
                $"from {_tableName} t " +
                $"left join lists ls on ls.Id = t.listId " +
                $"left join projects p on p.Id = ls.projectId " +
                $"left join task_members tm on  t.id = tm.taskId " +
                $"left join users u on tm.userid = u.id " +
                $"left join task_labels tl on t.id = tl.taskid " +
                $"left join labels lb on tl.labelid = lb.id " +            
                $"{whereStatement} ;";         
        }

        #region Members join

        private async Task<string> ConstructQueryToAddTaskMembersAsync(string taskId, IEnumerable<string> usersIds)
        {       
            var alreadyAddedMembersIds = await GetTaskMembersIdsAsync(taskId);

            var usersIdsToAdd = usersIds.Except(alreadyAddedMembersIds);
            
            if(!usersIdsToAdd.Any())
                return string.Empty;

            string insertQuery = $"insert into {_membersJoinTableName} (taskid, userid) values " + 
                $"{string.Join(", ", usersIdsToAdd.Select(id => $"('{taskId}', '{id}') "))} ; ";

            return insertQuery;              
        }

        private string ConstructQueryToDeleteTaskMembers(string taskId, IEnumerable<string> usersIds)
        {
            string deleteQuery = $"delete from {_membersJoinTableName} where " + 
                $"{string.Join(" OR ", usersIds.Select(id => $"(taskid = '{taskId}' AND userid = '{id}') "))} ;";

            return deleteQuery; 
        }

        public Task<IEnumerable<string>> GetTaskMembersIdsAsync(string taskId)
        {
            string query = $"select userid from {_membersJoinTableName} where taskid = '{taskId}'; ";
            return _connection.QueryAsync<string>(query);
        }

        #endregion Members join
        
        #region Labels join

        private async Task<string> ConstructQueryToAddTaskLabelsAsync(string taskId, IEnumerable<string> labelsIds)
        {
            var alreadyAddedLabelsIds = await GetTaskLabelsIdsAsync(taskId);

            var labelsIdsToAdd = labelsIds.Except(alreadyAddedLabelsIds);
            
            if(!labelsIdsToAdd.Any())
                return string.Empty;

            string insertQuery = $"insert into {_labelsJoinTableName} (taskid, labelid) values " + 
                $"{string.Join(", ", labelsIdsToAdd.Select(id => $"('{taskId}', '{id}') "))} ; ";

            return insertQuery;              
        }

        private string ConstructQueryToDeleteTaskLabels(string taskId, IEnumerable<string> labelsIds)
        {
            string deleteQuery = $"delete from {_labelsJoinTableName} where " + 
                $"{string.Join(" OR ", labelsIds.Select(id => $"(taskid = '{taskId}' AND labelid = '{id}') "))} ; ";

            return deleteQuery; 
        }

        private string ConstructQueryToDeleteTaskJoinData(string joinTableName, string taskId)
        {
            return $"delete from {joinTableName} where taskid = '{taskId}' ; ";            
        }        

        public Task<IEnumerable<string>> GetTaskLabelsIdsAsync(string taskId)
        {
            string query = $"select labelid from {_labelsJoinTableName} where taskid = '{taskId}'; ";
            return _connection.QueryAsync<string>(query);
        }


        #endregion Labels join

        private IEnumerable<TaskAggregate> AggregateQueryResult(IEnumerable<TaskQueryJoinedResult> resultsCollection)
        {
            var aggregatedTasks = new List<TaskAggregate>();
            
            if(resultsCollection == null || !resultsCollection.Any())
                return aggregatedTasks;

            var groupedResults = resultsCollection.GroupBy(res => res.Id);

            HashSet<string> labelsIds = new HashSet<string>();
            HashSet<string> membersIds = new HashSet<string>();

            foreach(var taskGroupedResults in groupedResults)
            {
                var firstRecord = taskGroupedResults.First();

                var taskAggregate = _mapper.Map<TaskQueryJoinedResult, TaskAggregate>(firstRecord);

                var aggregateLabels = new List<LabelModel>();
                var aggregateMembers = new List<UserModel>();

                foreach(var record in taskGroupedResults)
                {
                    string labelId = record.LabelId;
                    string userId = record.UserId;                   

                    if(!string.IsNullOrWhiteSpace(labelId) && !CheckIsExistsOrAdd(labelId, labelsIds))
                    {
                        var label = _mapper.Map<TaskQueryJoinedResult, LabelModel>(record);
                        aggregateLabels.Add(label);
                    }

                    if(!string.IsNullOrWhiteSpace(userId) && !CheckIsExistsOrAdd(userId, membersIds))
                    {
                        var member = _mapper.Map<TaskQueryJoinedResult, UserModel>(record);
                        aggregateMembers.Add(member);
                    }
                }

                taskAggregate.Labels = aggregateLabels;
                taskAggregate.Members = aggregateMembers;

                aggregatedTasks.Add(taskAggregate);

                labelsIds.Clear();
                membersIds.Clear();
            }

            return aggregatedTasks;
        }

        private bool CheckIsExistsOrAdd(string record, HashSet<string> set)
        {
            if(set.Contains(record))
                return true;

            set.Add(record);
            return false;
        }          
    }
}