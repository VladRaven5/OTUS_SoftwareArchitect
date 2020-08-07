using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace UsersService
{
    [ApiController]
    [Route("svc/users")]
    public class UsersInternalController : ControllerBase
    {
        private readonly UsersManager _userService;

        public UsersInternalController(UsersManager userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<ActionResult<UserModel>> CreateUser([FromQuery] CreateUserDto createDto)
        {
            try
            {
                var user = await _userService.CreateUserAsync(createDto.Username);
                return user;
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }           
        }
    }
}