using OTUS_SoftwareArchitect_Client.DTO;
using OTUS_SoftwareArchitect_Client.DTO.ProjectDtos;
using OTUS_SoftwareArchitect_Client.Models;
using OTUS_SoftwareArchitect_Client.Models.ProjectModels;
using OTUS_SoftwareArchitect_Client.Models.TaskModels;
using Refit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OTUS_SoftwareArchitect_Client.Networking
{
    public interface IWebApi
    {      
        #region Auth
        [Post("/login")]
        Task<string> Login([Body] LoginDto loginDto);

        [Post("/register")]
        Task<string> Register([Body] RegisterDto dto);

        [Get("/logout")]
        Task<string> Logout();

        [Get("/auth")]
        Task<string> Auth();

        #endregion Auth

        #region Users

        [Get("/users")]
        Task<IEnumerable<UserModel>> GetUsers();


        #endregion Users

        #region Tasks

        [Get("/tasks/my")]
        //[Headers("X-UserId: 2                                   ")]
        Task<IEnumerable<TaskModel>> GetMyTasks(/*[Header("X-UserId")] string userId*/);

        #endregion Tasks

        #region Projects

        [Get("/projects")]
        Task<IEnumerable<ProjectModel>> GetProjects();

        [Get("/projects/{projectId}")]
        Task<ProjectModel> GetProject(string projectId);

        [Post("/projects")]
        Task<ProjectModel> CreateProject(
            [Header(Constants.RequestIdHeaderName)] string reqiestId,
            [Body] CreateProjectDto dto);

        [Put("/projects")]
        Task<ProjectModel> UpdateProject([Body] UpdateProjectDto dto);


        [Delete("/projects/{projectId}")]
        Task DeleteProject(string projectId);

        #endregion Projects

        #region Project members

        [Get("/project-members")]
        Task<IEnumerable<ProjectMemberModel>> GetProjectMembers(string projectId);

        [Post("/project-members")]
        Task<string> AddMemberToProject(
            [Header(Constants.RequestIdHeaderName)] string requestId, 
            [Body] CreateUpdateProjectMemberDto dto);

        [Delete("/project-members")]
        Task RemoveMemberFromProject([Query] DeleteProjectMemberDto dto);


        #endregion Project members




    }

    public interface ILocalWebApi
    {
        #region Auth
        [Post("/login")]
        Task<string> Login([Body] LoginDto loginDto);

        [Post("/register")]
        Task<string> Register([Body] RegisterDto dto);

        [Get("/logout")]
        Task<string> Logout();

        [Get("/auth")]
        Task<string> Auth();

        #endregion Auth

        #region Tasks

        [Get("/my")]
        //[Headers("X-UserId: 2                                   ")]
        Task<IEnumerable<TaskModel>> GetMyTasks([Header("X-UserId")] string userId);

        #endregion Tasks




    }
}
