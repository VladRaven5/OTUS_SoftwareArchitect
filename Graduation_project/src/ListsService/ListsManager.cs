using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shared;

namespace ListsService
{
    public class ListsManager : DomainManagerBase
    {
        private readonly RequestsRepository _requestsRepository;
        private readonly ListsRepository _listsRepository;
        private readonly ProjectsRepository _projectsRepository;

        public ListsManager(RequestsRepository requestsRepository,
            ListsRepository listsRepository, ProjectsRepository projectsRepository,
            RabbitMqTopicManager rabbitMq) : base(requestsRepository)
        {
            _requestsRepository = requestsRepository;
            _listsRepository = listsRepository;
            _projectsRepository = projectsRepository;
        }

        public Task<IEnumerable<ListProjectAggregate>> GetAllListsAsync()
        {
            return _listsRepository.GetListsAsync();
        }

        public async Task<ListProjectAggregate> GetListByIdAsync(string listId)
        {
             var list = await _listsRepository.GetListByIdAsync(listId);
             if(list == null)
             {
                 throw new NotFoundException($"List with id {listId} not found");
             }
             return list;
        }

        public async Task<ListProjectAggregate> CreateListAsync(ListModel newList, string requestId)
        {
            if(!(await CheckAndSaveRequestIdAsync(requestId)))
            {
                throw new AlreadyHandledException();
            }

            try
            {
                var project = await _projectsRepository.GetProjectAsync(newList.ProjectId);
                if(project == null)
                {
                    throw new NotFoundException($"Project with id {newList.ProjectId} not found");
                }

                newList.Init();

                var outboxMessage = OutboxMessageModel.Create(
                    new ListCreatedMessage
                    {
                        ListId = newList.Id,
                        ProjectId = newList.ProjectId,
                        Title = newList.Title
                    }, Topics.Lists, MessageActions.Created);

                return await _listsRepository.CreateListAsync(newList, outboxMessage);
            }
            catch(Exception)
            {
                //rollback request id
                await _requestsRepository.DeleteRequestIdAsync(requestId);
                throw;
            }
        }

        public async Task<ListProjectAggregate> UpdateListAsync(ListModel updatingList)
        {
            ListModel currentList = await _listsRepository.GetListByIdAsync(updatingList.Id);
            if(currentList == null)
            {
                throw new NotFoundException($"List with id = {updatingList.Id} not found");
            }

            if(currentList.Version != updatingList.Version)
            {
                throw new VersionsNotMatchException();
            }

            var outboxMessage = OutboxMessageModel.Create(
                new ListUpdatedMessage
                {
                    ListId = updatingList.Id,
                    Title = updatingList.Title
                }, Topics.Lists, MessageActions.Updated);

            return await _listsRepository.UpdateListAsync(updatingList, outboxMessage);
        }
        public Task DeleteListAsync(string listId)
        {
            var outboxMessage = OutboxMessageModel.Create(
                new ListDeletedMessage
                {
                    ListId = listId,
                }, Topics.Lists, MessageActions.Deleted);

            return _listsRepository.DeleteListAsync(listId, outboxMessage);
        }        
    }
}