using OTUS_SoftwareArchitect_Client.DTO;
using OTUS_SoftwareArchitect_Client.Models;
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

        [Get("/my")]
        //[Headers("X-UserId: 2                                   ")]
        Task<IEnumerable<TaskModel>> GetMyTasks([Header("X-UserId")] string userId);

        #endregion Tasks




    }
}
