using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Shared;

namespace ProjectsService
{
    [ApiController]
    [Route("")]
    public class ExternalController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ProjectsManager _projectsManager;

        public ExternalController(IMapper mapper, ProjectsManager projectsManager)
        {
            _mapper = mapper;
            _projectsManager = projectsManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectModel>>> GetProjects()
        {
            return Ok(await _projectsManager.GetAllProjectsAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectModel>> GetProject(string id)
        {
            try
            {
                return await _projectsManager.GetProjectByIdAsync(id);
            }
            catch(NotFoundException nfe)
            {
                return NotFound(nfe.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<ProjectModel>> CreateProject([FromBody] CreateProjectDto createDto)
        {
            if(!Request.Headers.TryGetValue(Constants.RequestIdHeaderName, out StringValues requestIdValue))
            {
                return BadRequest($"Header {Constants.RequestIdHeaderName} must be specified");
            }

            string requsetId = requestIdValue.ToString();

            try
            {
                var creatingProject = _mapper.Map<CreateProjectDto, ProjectModel>(createDto);
                return Ok(await _projectsManager.CreateProjectAsync(creatingProject, requsetId));
            }
            catch(AlreadyHandledException)
            {
                return Accepted("Request is already handled");
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut]
        public async Task<ActionResult<ProjectModel>> UpdateProject([FromBody] UpdateProjectDto updateDto)
        {
            try
            {
                var updateProject = _mapper.Map<UpdateProjectDto, ProjectModel>(updateDto);
                return Ok(await _projectsManager.UpdateProjectAsync(updateProject));
            }
            catch(VersionsNotMatchException ve)
            {
                return Conflict(ve.Message);
            }
            catch(NotFoundException nfe)
            {
                return NotFound(nfe.Message);
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProject(string id)
        {
            await _projectsManager.DeleteProjectAsync(id);
            return Ok();
        }
    }
}