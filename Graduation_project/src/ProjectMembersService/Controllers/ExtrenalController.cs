using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Shared;

namespace ProjectMembersService
{
    [ApiController]
    [Route("")]
    public class ExternalController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ProjectMembersManager _projectMembersManager;

        public ExternalController(IMapper mapper, ProjectMembersManager projectMembersManager)
        {
            _mapper = mapper;
            _projectMembersManager = projectMembersManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectMemberAggregate>>> GetProjectsMembers(string projectId, string userId)
        {
            return Ok(await _projectMembersManager.GetProjectsMembersAsync(projectId, userId));
        }

        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<ProjectMemberAggregate>>> GetMyProjects()
        {
            if(!Request.Headers.TryGetValue(Constants.UserIdHeaderName, out StringValues userIdValue))
            {
                return BadRequest($"Header {Constants.UserIdHeaderName} must be specified");
            }

            string userId = userIdValue.ToString();

            return Ok(await _projectMembersManager.GetProjectsMembersAsync(userId: userId));            
        }

        [HttpPost]
        public async Task<ActionResult<ProjectMemberAggregate>> AddMemberToProject([FromBody] ProjectMemberDto projectMemberDto)
        {
            if(!Request.Headers.TryGetValue(Constants.RequestIdHeaderName, out StringValues requestIdValue))
            {
                return BadRequest($"{Constants.RequestIdHeaderName} header must be specified!");
            }

            string requestId = requestIdValue.ToString();

            var creatingMember = _mapper.Map<ProjectMemberDto, ProjectMemberModel>(projectMemberDto);
            try
            {
                await _projectMembersManager.AddMemberToProjectAsync(creatingMember, requestId);
                return Ok("Success");
            }
            catch(AlreadyHandledException ahe)
            {
                return Accepted(ahe.Message);
            }
            catch(NotFoundException nfe)
            {
                return NotFound(nfe.Message);
            }   
            catch(EntityExistsException e)
            {
                return BadRequest(e.Message);
            }         
        }

        [HttpPut("change-role")]
        public async Task<ActionResult> UpdateProjectMemberRole(ProjectMemberDto updatingDto)
        {
            try
            {
                var updatingProjectMember = _mapper.Map<ProjectMemberDto, ProjectMemberModel>(updatingDto);
                await _projectMembersManager.UpdateProjectMebmerRoleAsync(updatingProjectMember);
                return Ok("Success");
            }
            catch(VersionsNotMatchException vme)
            {
                return Conflict(vme.Message);
            }
            catch(NotFoundException nfe)
            {
                return NotFound(nfe.Message);
            }
        }   

        [HttpDelete]
        public async Task<ActionResult> RemoveMemberFromProject([FromQuery] ProjectUserPairDto projectUserDto)
        {
            await _projectMembersManager.RemoveMemberFromProjectAsync(projectUserDto.ProjectId, projectUserDto.UserId);
            return Ok("Success");
        }    
    }
}