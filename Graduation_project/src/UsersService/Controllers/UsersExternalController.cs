using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace UsersService
{
    [ApiController]
    [Route("")]
    public class UsersExternalController : ControllerBase
    {
        private readonly UsersManager _userService;
        private readonly IMapper _mapper;

        public UsersExternalController(UsersManager userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
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
        public async Task<ActionResult<UserModel>> UpdateUser([FromBody] UpdateUserDto updatingDto)
        {
            try
            {               
                string currentUserId = Request.Headers[Constants.UserIdHeaderName];
                if(currentUserId != updatingDto.Id)
                {
                    return StatusCode((int)HttpStatusCode.Forbidden);
                }

                var updatingModel = _mapper.Map<UpdateUserDto, UserModel>(updatingDto);

                return await _userService.UpdateUserAsync(updatingModel);
            }
            catch(VersionsNotMatchException vnme)
            {
                return Conflict(vnme.Message);
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