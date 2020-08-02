using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
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

        [Authorize]
        [HttpGet("auth")]
        public ActionResult Authenticate()
        {
            return Ok(User.Identity.Name);
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                var user = await _authenticationService.RegisterAsync(registerDto.Username, registerDto.Login, registerDto.Password);
                await AuthenticateUser(user.Id);
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }                       

            return Ok();
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDto loginDto)
        {
            string login = loginDto.Login;
            string password = loginDto.Password;

            UserAuthInfo user = await _authenticationService.LoginAsync(login, password);
            if(user == null)
            {
                NotFound("user with this credentials not found");
            }

            await AuthenticateUser(user.Id);        

            return Ok();
        }

        [Authorize]
        [HttpGet("logout")]
        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            
            return Ok();
        } 

        private Task AuthenticateUser(string userId)
        {
            var claims = new List<Claim> 
            {
                new Claim(ClaimTypes.Name,  userId)
            };

            ClaimsIdentity identity = new ClaimsIdentity(claims, "User Identity");            

            return HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));
        }
    }
}