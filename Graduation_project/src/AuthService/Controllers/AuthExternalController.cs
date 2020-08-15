using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace AuthService
{
    [ApiController]
    [Route("")]
    public class AuthExternalController : ControllerBase
    {
        private readonly AuthenticationService _authenticationService;

        public AuthExternalController(AuthenticationService authenticationService)
        {            
            _authenticationService = authenticationService;
        }

        [Authorize]
        [HttpGet("auth")]
        public ActionResult Authenticate()
        {
            string userId = User.Identity.Name;
            SetUserIdHeader(userId);

            return Ok("Success");
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                var user = await _authenticationService.RegisterAsync(registerDto.Username, registerDto.Login, registerDto.Password);
                //await AuthenticateUser(user.Id);
                //SetUserIdHeader(user.Id);
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }                       

            return Ok("Success");
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDto loginDto)
        {
            string login = loginDto.Login;
            string password = loginDto.Password;

            UserAuthInfo user = await _authenticationService.LoginAsync(login, password);
            if(user == null)
            {
                return NotFound("user with this credentials not found");
            }

            await AuthenticateUser(user.Id);

            SetUserIdHeader(user.Id);      

            return Ok(user.Id);
        }

        [Authorize]
        [HttpGet("logout")]
        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
                        
            return Ok("Success");
        } 

        private void SetUserIdHeader(string userId)
        {
            Response.Headers.Add(Constants.UserIdHeaderName, userId);
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