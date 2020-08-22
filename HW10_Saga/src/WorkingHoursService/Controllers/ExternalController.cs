using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Shared;

namespace WorkingHoursService
{
    [ApiController]
    [Route("")]
    public class ExternalController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly TaskUserWorkingHoursManager _workingHoursService;

        public ExternalController(IMapper mapper, TaskUserWorkingHoursManager workingHoursService)
        {
            _mapper = mapper;
            _workingHoursService = workingHoursService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberWorkingHoursAggregate>>> GetProjectWorkingHours(string projectId, string taskId, string userId)
        {
            return Ok(await _workingHoursService.GetProjectWorkingHoursAsync(projectId, taskId, userId));
        }

        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<MemberWorkingHoursAggregate>>> GetMyWorkingHours()
        {
            if(!Request.Headers.TryGetValue(Constants.UserIdHeaderName, out StringValues userIdValue))
            {
                return BadRequest($"Header {Constants.UserIdHeaderName} must be specified");
            }

            string userId = userIdValue.ToString();

            return Ok(await _workingHoursService.GetUserHoursAsync(userId));
        }

        [HttpGet("{recordId}")]
        public async Task<ActionResult<MemberWorkingHoursAggregate>> GetRecordById(string recordId)
        {
            try
            {
                return Ok(await _workingHoursService.GetRecordByIdAsync(recordId));               
            }
            catch(NotFoundException nfe)
            {
                return NotFound(nfe.Message);
            }
            catch(Exception e)
            {
                return BadRequest($"{e.GetType()} : {e.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<MemberWorkingHoursAggregate>> CreateRecord([FromBody] CreateWorkingHoursRecordDto creationDto)
        {
            if(!Request.Headers.TryGetValue(Constants.RequestIdHeaderName, out StringValues requestIdValue))
            {
                return BadRequest($"{Constants.RequestIdHeaderName} header must be specified!");
            }

            if(!Request.Headers.TryGetValue(Constants.UserIdHeaderName, out StringValues userIdValue))
            {
                return BadRequest($"{Constants.UserIdHeaderName} header must be specified!");
            }

            string requestId = requestIdValue.ToString();
            string userId = userIdValue.ToString();

            try
            {
                TaskUserWorkingHoursRecord newRecord = _mapper.Map<CreateWorkingHoursRecordDto, TaskUserWorkingHoursRecord>(creationDto);
                newRecord.UserId = userId;
                
                var createdRecord = await _workingHoursService.CreateRecordAsync(newRecord, requestId);
                
                return Ok(createdRecord);
            }
            catch(AlreadyHandledException ahe)
            {
                return Accepted(ahe.Message);
            }
            catch(NotFoundException nfe)
            {
                return NotFound(nfe.Message);
            }
            catch(Exception e)
            {
                return BadRequest($"{e.GetType()} : {e.Message}");
            }         
        }

        [HttpPut]
        public async Task<ActionResult<MemberWorkingHoursAggregate>> UpdateRecord([FromBody] UpdateWorkingHoursRecordDto updateDto)
        {
            try
            {
                TaskUserWorkingHoursRecord updatingRecord = _mapper.Map<UpdateWorkingHoursRecordDto, TaskUserWorkingHoursRecord>(updateDto);
                var updatedRecord = await _workingHoursService.UpdateRecordAsync(updatingRecord);
                
                return Ok(updatedRecord);
            }
            catch(VersionsNotMatchException e)
            {
                return Conflict(e.Message);
            }
            catch(NotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch(Exception e)
            {
                return BadRequest($"{e.GetType()} : {e.Message}");
            }     
        }

        [HttpDelete("{recordId}")]
        public async Task<ActionResult> DeleteRecord(string recordId)
        {
            await _workingHoursService.DeleteRecordAsync(recordId);

            return Ok();
        }
    }
}