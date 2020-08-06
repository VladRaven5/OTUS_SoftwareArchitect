using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Shared;

namespace ProjectsService
{
    public class ProjectsRepository : BaseDapperRepository
    {
        protected override string _tableName => "projects";

        public ProjectsRepository(PostgresConnectionManager connectionManager)
            : base(connectionManager.GetConnection())
        {
        }

        public Task<IEnumerable<ProjectModel>> GetProjectsAsync()
        {
            string query = $"select * from {_tableName};";
            return _connection.QueryAsync<ProjectModel>(query);
        }

        public Task<ProjectModel> GetProjectByIdAsync(string projectId)
        {
            return GetModelByIdAsync<ProjectModel>(projectId);
        }

        public async Task<ProjectModel> CreateProjectAsync(ProjectModel newProject)
        {
            newProject.Init();

            string insertQuery = $"insert into {_tableName} (id, title, description, createddate, begindate, enddate, version) "
                + $"values('{newProject.Id}', '{newProject.Title}', '{newProject.Description}', '{newProject.Description}', '{newProject.CreatedDate}', '{newProject.BeginDate}', '{newProject.EndDate}', {newProject.Version});";

            int res = await _connection.ExecuteAsync(insertQuery);

            if(res <= 0)
            {
                throw new DatabaseException("Create project failed");
            }            

            return await GetProjectByIdAsync(newProject.Id);
        }

        public async Task<ProjectModel> UpdateProjectAsync(ProjectModel updatedProject)
        {
            int newVersion = updatedProject.Version + 1;

            string updateQuery = $"update {_tableName} set " + 
                $"title = '{updatedProject.Title}', " +
                $"description = '{updatedProject.Description}', " +
                $"begindate = '{updatedProject.BeginDate}', " +
                $"enddate = '{updatedProject.EndDate}', " +
                $"version = {newVersion} " +
                $"where id = '{updatedProject.Id}';";

            int res = await _connection.ExecuteAsync(updateQuery);

            if(res <= 0)
            {
                throw new DatabaseException("Update project failed");
            }

            return await GetProjectByIdAsync(updatedProject.Id);
        }

        public Task DeleteProjectAsync(string projectId)
        {
            return DeleteModelAsync(projectId);          
        }
    }
}