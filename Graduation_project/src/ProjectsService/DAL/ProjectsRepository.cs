using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Shared;

namespace ProjectsService
{
    public class ProjectsRepository : IDisposable
    {
        private const string _projectsTableName = "projects";
        private readonly IDbConnection _connection;

        public ProjectsRepository(PostgresConnectionManager connectionManager)
        {
            _connection = connectionManager.GetConnection();
        }

        public Task<IEnumerable<ProjectModel>> GetProjectsAsync()
        {
            string query = $"select * from {_projectsTableName};";
            return _connection.QueryAsync<ProjectModel>(query);
        }

        public Task<ProjectModel> GetProjectByIdAsync(string projectId)
        {
            string query = $"select * from {_projectsTableName} where id = '{projectId}' limit 1;";
            return _connection.QueryFirstOrDefaultAsync<ProjectModel>(query);
        }

        public async Task<ProjectModel> CreateProjectAsync(ProjectModel newProject)
        {
            newProject.Init();

            string insertQuery = $"insert into {_projectsTableName} (id, title, description, createddate, begindate, enddate, version) "
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

            string updateQuery = $"update {_projectsTableName} set " + 
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

        public async Task DeleteProjectAsync(string projectId)
        {
            string deleteQuery = $"delete from {_projectsTableName} where id = '{projectId}';";
            await _connection.ExecuteAsync(deleteQuery);              
        }
        

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}