using OTUS_SoftwareArchitect_Client.DTO;
using OTUS_SoftwareArchitect_Client.DTO.ProjectDtos;
using OTUS_SoftwareArchitect_Client.Models;
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

        #region Tasks

        [Get("/tasks/my")]
        //[Headers("X-UserId: 2                                   ")]
        Task<IEnumerable<TaskModel>> GetMyTasks(/*[Header("X-UserId")] string userId*/);

        #endregion Tasks

        #region Projects

        [Get("/projects")]
        Task<IEnumerable<ProjectModel>> GetProjects();

        [Post("/projects")]
        Task<ProjectModel> CreateProject(
            [Header(Constants.RequestIdHeaderName)] string reqiestId,
            [Body] CreateProjectDto dto);

        #endregion Projects




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
