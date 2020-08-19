using System;
using System.Collections.Generic;
using System.Linq;
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

        [HttpPost]
        public async Task<ActionResult<UserModel>> CreateUser([FromQuery] CreateUserDto createDto)
        {
            try
            {
                var creatingUser = _mapper.Map<CreateUserDto, UserModel>(createDto);
                var user = await _userService.CreateUserAsync(creatingUser);
                return user;
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }           
        }

        [HttpPut]
        public async Task<ActionResult<UserModel>> UpdateUser([FromBody] UpdateUserDto updatingDto)
        {
            try
            {
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

        [HttpDelete("clear")]
        public async Task<ActionResult> DeleteAllUsers()
        {
            await _userService.DeleteAllUsersAsync();
            return Ok();            
        }


        [HttpGet("sharding-map")]
        public async Task<ActionResult<IEnumerable<UserShardRecord>>> GetShardingMap()
        {
            return Ok(await _userService.GetShardingMapAsync());
        }

        [HttpGet("sharding-summary")]
        public async Task<ActionResult<IEnumerable<ShardingSummaryRecordDto>>> GetShardingSummary()
        {
            var userSharding = await _userService.GetShardingMapAsync();

            var summaryrecords = userSharding
                .GroupBy(us => us.ShardKey)
                .Select(g => new ShardingSummaryRecordDto
                {
                    ShardKey = g.Key,
                    Count = g.Count()                    
                })
                .OrderBy(s => s.ShardKey)
                .ToList();

            return Ok(summaryrecords);
        }

        [HttpGet("user-shard")]
        public async Task<ActionResult<string>> GetUserShard(string userId)
        {
            var userSharding = await _userService.GetShardingMapAsync();
            var userShard = userSharding.FirstOrDefault(us => us.UserId == userId)?.ShardKey ?? "NONE";
            return Ok(userShard);
        }
    }
}