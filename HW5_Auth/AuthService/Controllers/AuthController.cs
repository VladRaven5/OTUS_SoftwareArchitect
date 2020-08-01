using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace AuthService
{
    [ApiController]
    [Route("")]
    public class AuthController : ControllerBase
    {
        private readonly AuthenticationService _authenticationService;

        public AuthController(AuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpGet("auth")]
        public async Task<ActionResult> Authenticate(string sessionId)
        {
            return await _authenticationService.AuthenticateAsync(sessionId) 
                ? (ActionResult)Ok()
                : Unauthorized("You must login at /login path");
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDto loginDto)
        {
            string login = loginDto.Login;
            string password = loginDto.Password;

            if(string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                BadRequest("Login and password can't be empty");
            }

            string sessionId = await _authenticationService.LoginAsync(login, password);

            return string.IsNullOrWhiteSpace(sessionId)
                ? (ActionResult)Forbid()
                : Ok();
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterDto registerDto)
        {
            string login = registerDto.Login;
            string password = registerDto.Password;
            string username = registerDto.Username;

            if(string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(username))
            {
                BadRequest("Login, password or username can't be empty");
            }

            await _authenticationService.RegisterAsync(username, login, password);

            return Ok();
        }
    }
}