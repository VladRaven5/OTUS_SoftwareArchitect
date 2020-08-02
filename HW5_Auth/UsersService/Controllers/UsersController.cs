using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UsersService
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        [Authorize]
        [HttpGet("test")]
        public ActionResult Test()
        {
            var a = User.Identity.Name;
            return Ok(a);
        }


        [HttpGet("login")]
        public Task AuthenticateUser()
        {
            string userId = "12345";

            var claims = new List<Claim> 
            {
                new Claim(ClaimTypes.Name,  userId)
            };

            ClaimsIdentity identity = new ClaimsIdentity(claims, "User Identity");            

            return HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));
        }

        [HttpGet("logout")]
        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            
            return Ok();
        }
    }
}