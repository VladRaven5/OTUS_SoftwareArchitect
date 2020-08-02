using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace UsersService
{
    [ApiController]
    [Route("svc/users")]
    public class UsersInternalController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersInternalController(UserService userService)
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