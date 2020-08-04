using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace WorkingHoursService
{
    [ApiController]
    [Route("")]
    public class ExternalController : ControllerBase
    {
        private const string _requestIdHeaderName = "X-RequestId";

        private readonly IMapper _mapper;
        private readonly TaskUserWorkingHoursService _workingHoursService;

        public ExternalController(IMapper mapper, TaskUserWorkingHoursService workingHoursService)
        {
            _mapper = mapper;
            _workingHoursService = workingHoursService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskUserWorkingHoursRecord>>> GetRecords()
        {
            return Ok(await _workingHoursService.GetAllRecordsAsync());
        }

        [HttpGet("{recordId}")]
        public async Task<ActionResult<TaskUserWorkingHoursRecord>> GetRecordById(string recordId)
        {
            try
            {
                var record = await _workingHoursService.GetRecordByIdAsync(recordId);
                
                if(record == null)
                {
                    return NotFound($"Record with id = {recordId}");
                }

                return Ok(record);
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<TaskUserWorkingHoursRecord>> CreateRecord([FromBody] CreateWorkingHoursRecordDto creationDto)
        {
            if(!Request.Headers.TryGetValue(_requestIdHeaderName, out StringValues requestIdValue))
            {
                return BadRequest($"{_requestIdHeaderName} header must be specified!");
            }

            string requestId = requestIdValue.ToString();

            try
            {
                TaskUserWorkingHoursRecord newRecord = _mapper.Map<CreateWorkingHoursRecordDto, TaskUserWorkingHoursRecord>(creationDto);
                var createdRecord = await _workingHoursService.CreateRecordAsync(newRecord, requestId);
                
                return Ok(createdRecord);
            }
            catch(AlreadyHandledException e)
            {
                return Accepted(e.Message);
            }
            catch(Exception e)
            {
                return BadRequest($"{e.GetType()} : {e.Message}");
            }         
        }

        [HttpPut]
        public async Task<ActionResult<TaskUserWorkingHoursRecord>> UpdateRecord([FromBody] UpdateWorkingHoursRecordDto updateDto)
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