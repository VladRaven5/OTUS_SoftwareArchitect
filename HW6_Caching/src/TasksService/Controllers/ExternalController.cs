using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Shared;

namespace TasksService
{
    [ApiController]
    [Route("")]
    public class ExternalController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly TasksManager _tasksManager;
        private HttpStatusCode _cacheFoundCode = HttpStatusCode.Accepted;


        public ExternalController(IMapper mapper, TasksManager tasksManager)
        {
            _mapper = mapper;
            _tasksManager = tasksManager;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskAggregate>>> GetTasks()
        {
            return Ok(await _tasksManager.GetTasksAsync());
        }

        [HttpGet("{taskId}")]
        public async Task<ActionResult<TaskAggregate>> GetTask(string taskId)
        {
            try
            {  
                var result = await _tasksManager.GetTaskAsync(taskId);
                var task = result.task;
                if(result.fromCache)
                {
                    Response.StatusCode = (int)_cacheFoundCode;
                    return task;;
                }
                return Ok(task);
            }
            catch(NotFoundException nfe)
            {
                return NotFound(nfe.Message);
            }
            catch(Exception e)
            {
                return BadRequest($"{e.GetType().Name} : {e.Message}\n{e.StackTrace}");
            }
        }

        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<TaskAggregate>>> GetMyTasks()
        {
            if(!Request.Headers.TryGetValue(Constants.UserIdHeaderName, out StringValues userIdValue))
            {
                return BadRequest($"Header {Constants.UserIdHeaderName} must be specified");
            }

            string userId = userIdValue.ToString();

            try
            {
                var result = await _tasksManager.GetUserTasksAsync(userId);
                var list = result.list;
                if(result.fromCache)
                {
                    Response.StatusCode = (int)_cacheFoundCode;
                    return new ActionResult<IEnumerable<TaskAggregate>>(list);
                }
                return Ok(list);
            }
            catch(Exception e)
            {
                return BadRequest($"{e.GetType().Name}: {e.Message}\n{e.StackTrace}");
            }            
        }

        [HttpPost("filter")]
        public async Task<ActionResult<IEnumerable<TaskAggregate>>> FilterTask([FromBody] FilterTaskArgs dto)
        {
            var result = await _tasksManager.FilterTasksAsync(dto);
            var list = result.list;
            if(result.fromCache)
            {
                Response.StatusCode = (int)_cacheFoundCode;
                return new ActionResult<IEnumerable<TaskAggregate>>(list);
            }
            return Ok(list);
        }

        [HttpPost]
        public async Task<ActionResult<TaskAggregate>> CreateTask([FromBody] CreateTaskDto createDto)
        {
            if(!Request.Headers.TryGetValue(Constants.RequestIdHeaderName, out StringValues requestIdValue))
            {
                return BadRequest($"Header {Constants.RequestIdHeaderName} must be specified");
            }

            string requestId = requestIdValue.ToString();
            var members = createDto.MembersIds ?? new List<string>();
            var labels = createDto.LabelsIds ?? new List<string>();

            try
            {
                var creatingTask = _mapper.Map<CreateTaskDto, TaskModel>(createDto);
                return Ok(await _tasksManager.CreateTaskAsync(creatingTask, members, labels, requestId));
            }
            catch(AlreadyHandledException)
            {
                return Accepted("Request is already handled");
            }
            catch(NotFoundException nfe)
            {
                return NotFound(nfe.Message);
            }
            catch(Exception e)
            {
                return BadRequest($"{e.Message}\n{e.StackTrace}");
            }
        }        

        [HttpPut]
        public async Task<ActionResult<TaskModel>> UpdateTask([FromBody] UpdateTaskDto updateDto)
        {
            try
            {
                var members = updateDto.MembersIds ?? new List<string>();
                var labels = updateDto.LabelsIds ?? new List<string>();

                var updateTask = _mapper.Map<UpdateTaskDto, TaskModel>(updateDto);
                return Ok(await _tasksManager.UpdateTaskAsync(updateTask, members, labels));
            }
            catch(VersionsNotMatchException ve)
            {
                return Conflict(ve.Message);
            }
            catch(NotFoundException nfe)
            {
                return NotFound(nfe.Message);
            }
            catch(ProhibitedException pe)
            {
                return BadRequest(pe.Message);
            }
            catch(Exception e)
            {
                return BadRequest($"{e.Message}\n{e.StackTrace}");
            }
        }


        [HttpDelete("{taskId}")]
        public async Task<ActionResult> DeleteTask(string taskId)
        {
            await _tasksManager.DeleteTaskAsync(taskId);
            return Ok("Success");
        }


        [HttpPost("set-cache")]
        public async Task<ActionResult> SetCache(string value)
        {
            await _tasksManager.SetCacheValue(value);
            return Ok();
        }

        [HttpGet("get-cache")]
        public async Task<ActionResult<string>> GetCache()
        {
            return Ok(await _tasksManager.GetCacheValue());
        }
    }
}
