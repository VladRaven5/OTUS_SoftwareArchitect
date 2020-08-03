using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UsersService
{
    [ApiController]
    [Route("users")]
    public class UsersExternalController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersExternalController(UserService userService)
        {
            _userService = userService;
        }

        [Authorize]
        [HttpGet]
        public async Task<List<UserModel>> GetUsers()
        {
            return await _userService.GetUsersAsync();
        }

        [Authorize]
        [HttpGet("{userId}")]
        public async Task<ActionResult<UserModel>> GetUser(string userId)
        {
            try
            {
                return await _userService.GetUserAsync(userId);
            }
            catch(Exception e)
            {
                return NotFound(e.Message);
            }          
        }

        [Authorize]
        [HttpPut]
        public async Task<ActionResult<UserModel>> UpdateUser(string userId, string username)
        {
            try
            {
                return await _userService.UpdateUserAsync(userId, username);
            }
            catch(Exception e)
            {
                return NotFound(e.Message);
            }   
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserModel>> GetMe()
        {
            var userId = User.Identity.Name;

            try
            {
                var user = await _userService.GetUserAsync(userId);
                return Ok(user);
            }
            catch(Exception e)
            {
                return NotFound(e.Message);
            }
        }

        [Authorize]
        [HttpDelete("{userId}")]
        public async Task<ActionResult> DeleteUser(string userId)
        {
            try
            {
                var deletionResult = await _userService.DeleteUserAsync(userId);
                
                if(!deletionResult.isSuccess)
                    return BadRequest(deletionResult.error);

                return Ok();
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}