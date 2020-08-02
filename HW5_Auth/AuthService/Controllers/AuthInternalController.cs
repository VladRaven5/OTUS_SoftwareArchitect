using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace AuthService
{
    [ApiController]
    [Route("svc/userauth")]
    public class AuthInternalController : ControllerBase
    {
        private readonly AuthenticationService _authenticationService;

        public AuthInternalController(AuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpDelete("{userId}")]
        public async Task<ActionResult> DeleteUserInfo(string userId)
        {
            try
            {
                await _authenticationService.DeleteUserAsync(userId);
                return Ok();
            }
            catch(Exception e)
            {
                return NotFound(e.Message);
            }            
        }
    }
}