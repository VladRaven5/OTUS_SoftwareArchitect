using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Shared;

namespace LabelsService
{
    [ApiController]
    [Route("")]
    public class ExternalController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly LabelsManager _labelManager;

        public ExternalController(IMapper mapper, LabelsManager labelManager)
        {
            _mapper = mapper;
            _labelManager = labelManager;
        }

        [HttpGet("{labelId}")]
        public async Task<ActionResult<LabelModel>> GetLabel(string labelId)
        {
            try
            {   
                var label = await _labelManager.GetLabelByIdAsync(labelId);
                return Ok(label);
            }
            catch(NotFoundException nfe)
            {
                return NotFound(nfe.Message);
            }          
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LabelModel>>> GetLabels()
        {
            return Ok(await _labelManager.GetAllLabelsAsync());
        }

        [HttpPost]
        public async Task<ActionResult<LabelModel>> CreateLabel([FromBody] CreateLabelDto createDto)
        {
            if(!Request.Headers.TryGetValue(Constants.RequestIdHeaderName, out StringValues requestIdValue))
            {
                return BadRequest($"Header {Constants.RequestIdHeaderName} must be specified");
            }

            string requestId = requestIdValue.ToString();

            try
            {
                var creatingLabel = _mapper.Map<CreateLabelDto, LabelModel>(createDto);
                var createdLabel =  await _labelManager.CreateLabelAsync(creatingLabel, requestId);
                
                return Ok(createdLabel);
            }
            catch(AlreadyHandledException)
            {
                return Accepted("Request is already handled");
            }
            catch(Exception e)
            {
                return BadRequest($"{e.Message}\n{e.StackTrace}");
            }
        }

        [HttpPut]
        public async Task<ActionResult<LabelModel>> UpdateLabel([FromBody] UpdateLabelDto updateDto)
        {
            try
            {
                var updatingLabel = _mapper.Map<UpdateLabelDto, LabelModel>(updateDto);
                var updatedLabel = await _labelManager.UpdateLabelAsync(updatingLabel);

                return Ok(updatedLabel);
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
                return BadRequest($"{e.Message}\n{e.StackTrace}");
            }
        }

        [HttpDelete("{labelId}")]
        public async Task<ActionResult> DeleteLabel(string labelId)
        {
            await _labelManager.DeleteLabelAsync(labelId);
            return Ok("Success");
        }
    }
}