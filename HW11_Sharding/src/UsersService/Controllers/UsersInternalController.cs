using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace UsersService
{
    [ApiController]
    [Route("svc/users")]
    public class UsersInternalController : ControllerBase
    {
        private readonly UsersManager _userService;
        private readonly IMapper _mapper;

        public UsersInternalController(UsersManager userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
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
    }
}