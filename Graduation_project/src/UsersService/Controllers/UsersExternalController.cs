using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace UsersService
{
    [ApiController]
    [Route("")]
    public class UsersExternalController : ControllerBase
    {
        private readonly UsersManager _userService;

        public UsersExternalController(UsersManager userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<List<UserModel>> GetUsers()
        {
            return await _userService.GetUsersAsync();
        }

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

        [HttpPut]
        public async Task<ActionResult<UserModel>> UpdateUser(string userId, string username)
        {
            try
            {
                string currentUserId = Request.Headers[Constants.UserIdHeaderName];
                if(currentUserId != userId)
                {
                    return Forbid();
                }

                return await _userService.UpdateUserAsync(userId, username);
            }
            catch(Exception e)
            {
                return NotFound(e.Message);
            }   
        }

        [HttpGet("me")]
        public async Task<ActionResult<UserModel>> GetMe()
        {
            try
            {
                string userId = Request.Headers[Constants.UserIdHeaderName];                              
                var user = await _userService.GetUserAsync(userId);
                return Ok(user);
            }
            catch(Exception e)
            {
                return NotFound(e.Message);
            }
        }

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