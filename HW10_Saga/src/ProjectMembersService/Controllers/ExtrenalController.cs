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

        /// <summary>
        /// Get members of the project or projects of the member, or just one project member.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectMemberAggregate>>> GetProjectsMembers(string projectId, string userId)
        {
            return Ok(await _projectMembersManager.GetProjectsMembersAsync(projectId, userId));
        }

        /// <summary>
        /// Get projects of current user (by X-UserId header value)
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Add member to project
        /// </summary>
        /// <param name="projectMemberDto"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Change member project role (0 - Implementer, 1 - Manager)
        /// </summary>
        /// <param name="updatingDto"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Remove member from project
        /// </summary>
        /// <param name="projectUserDto"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<ActionResult> RemoveMemberFromProject([FromQuery] ProjectUserPairDto projectUserDto)
        {
            await _projectMembersManager.RemoveMemberFromProjectAsync(projectUserDto.ProjectId, projectUserDto.UserId);
            return Ok("Success");
        }    
    }
}