using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Shared;

namespace ListsService
{
    [ApiController]
    [Route("")]
    public class ExternalController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ListsManager _listsManager;

        public ExternalController(IMapper mapper, ListsManager listsManager)
        {
            _mapper = mapper;
            _listsManager = listsManager;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<ListProjectAggregate>>> GetLists()
        {
            return Ok(await _listsManager.GetAllListsAsync()); 
        }

        [HttpGet("{listId}")]
        public async Task<ActionResult<ListProjectAggregate>> GetListById(string listId)
        {
            try
            {
                var list = await _listsManager.GetListByIdAsync(listId);
                return Ok(list);
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

        [HttpPost]
        public async Task<ActionResult<ListProjectAggregate>> CreateList([FromBody] CreateListDto createDto)
        {
            if(!Request.Headers.TryGetValue(Constants.RequestIdHeaderName, out StringValues requestIdValue))
            {
                return BadRequest($"Header {Constants.RequestIdHeaderName} must be specified");
            }

            string requestId = requestIdValue.ToString();

            try
            {
                var creatingList = _mapper.Map<CreateListDto, ListModel>(createDto);
                var createdList = await _listsManager.CreateListAsync(creatingList, requestId);
                return Ok(creatingList);
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
                return BadRequest($"{e.GetType()}: {e.Message}\n{e.StackTrace}");
            }
        }

        [HttpPut]
        public async Task<ActionResult<ListProjectAggregate>> UpdateList([FromBody] UpdateListDto updateDto)
        {
            try
            {
                var updatingList = _mapper.Map<UpdateListDto, ListModel>(updateDto);
                var updatedList = await _listsManager.UpdateListAsync(updatingList);
                return Ok(updatedList);
            }
            catch(NotFoundException nfe)
            {
                return NotFound(nfe.Message);
            }
            catch(VersionsNotMatchException vnme)
            {
                return Conflict(vnme.Message);
            }
            catch(Exception e)
            {
                return BadRequest($"{e.GetType()}: {e.Message}\n{e.StackTrace}");
            }
        }

        [HttpDelete("{listId}")]
        public async Task<ActionResult> DeleteList(string listId)
        {
            await _listsManager.DeleteListAsync(listId);
            return Ok();
        }
    }
}