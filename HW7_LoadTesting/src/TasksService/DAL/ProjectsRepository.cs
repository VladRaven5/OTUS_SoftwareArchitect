using System.Threading.Tasks;
using Dapper;
using Shared;

namespace TasksService
{
    public class ProjectsRepository : BaseDapperRepository
    {
        protected override string _tableName => "projects";        

        public ProjectsRepository(PostgresConnectionManager connectionManager)
            : base(connectionManager.GetConnection())
        {
        }

        public Task<ProjectModel> GetProjectAsync(string projectId)
        {
            return GetModelByIdAsync<ProjectModel>(projectId);
        }

        public async Task CreateOrUpdateProjectAsync(ProjectModel project)
        {
            var existingProject = await GetModelByIdAsync<ProjectModel>(project.Id);

            if(existingProject == null)
            {
                await CreateProjectAsync(project);
            }
            else
            {
                await UpdateProjectAsync(project);
            }
        }
        
        private async Task CreateProjectAsync(ProjectModel newProject)
        {
            string insertQuery = $"insert into {_tableName} (id, title) "
                + $"values('{newProject.Id}', '{newProject.Title}');";

            int res = await _connection.ExecuteAsync(insertQuery);

            if(res <= 0)
            {
                throw new DatabaseException("Create project failed");
            }
        }

        private async Task UpdateProjectAsync(ProjectModel project)
        {
            string updateQuery = $"update {_tableName} set title = '{project.Title}' where id = '{project.Id}';";

            int res = await _connection.ExecuteAsync(updateQuery);

            if(res <= 0)
            {
                throw new DatabaseException("Update project failed");
            }
        }

        public Task DeleteProjectAsync(string projectId)
        {
            return DeleteModelAsync(projectId);
        }
    }
}