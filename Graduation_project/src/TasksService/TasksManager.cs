using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Shared;

namespace TasksService
{
    public partial class TasksManager : DomainManagerBase
    {
        private readonly IEqualityComparer<BaseModel> _modelsByIdComparer = new BaseModelsEqualityComparer();
        private readonly TasksRepository _tasksRepository;
        private readonly RequestsRepository _requestsRepository;
        private readonly ListsRepository _listsRepository;
        private readonly UsersRepository _usersRepository;
        private readonly LabelsRepository _labelsRepository;
        private readonly ProjectMembersRepository _projectMembersRepository;
        private readonly IMapper _mapper;

        public TasksManager(TasksRepository tasksRepository, RequestsRepository requestsRepository,
            ListsRepository listsRepository, UsersRepository usersRepository, LabelsRepository labelsRepository,
            ProjectMembersRepository projectMembersRepository, IMapper mapper)
            : base(requestsRepository)
        {
            _tasksRepository = tasksRepository;
            _requestsRepository = requestsRepository;
            _listsRepository = listsRepository;
            _usersRepository = usersRepository;
            _labelsRepository = labelsRepository;
            _projectMembersRepository = projectMembersRepository;
            _mapper = mapper;
        }

        public async Task<TaskAggregate> GetTaskAsync(string taskId)
        {
            var task = await  _tasksRepository.GetTaskAsync(taskId);
            if(task == null)
             {
                 throw new NotFoundException($"Task with id {taskId} not found");
             }
            return task;
        }

        public Task<IEnumerable<TaskAggregate>> GetTasksAsync()
        {
            return _tasksRepository.GetAllTasksAsync();
        }

        public Task<IEnumerable<TaskAggregate>> GetUserTasksAsync(string userId)
        {
            return _tasksRepository.GetUserTasksAsync(userId);
        }

        public async Task<TaskAggregate> CreateTaskAsync(TaskModel newTask,
            IEnumerable<string> usersIds, IEnumerable<string> labelsIds, string requestId)
        {
            if(!(await CheckAndSaveRequestIdAsync(requestId)))
            {
                throw new AlreadyHandledException();
            }

            try
            {
                newTask.Init();

                TaskRelatedData taskData = await GetAndCheckTaskRelatedDataAsync(newTask, usersIds, labelsIds);
                
                var list = taskData.List;

                var taskCollections = new TaskCollections
                {
                    Members = usersIds,
                    Labels = labelsIds
                };

                var membersRecords = _mapper.Map<IEnumerable<UserModel>, IEnumerable<TaskUserRecord>>(taskData.Users);
                var labelsRecords = _mapper.Map<IEnumerable<LabelModel>, IEnumerable<TaskLabelRecord>>(taskData.Labels);

                var outboxMessage = OutboxMessageModel.Create(
                    new TaskCreatedMessage
                    {
                        TaskId = newTask.Id,
                        Title = newTask.Title,
                        ProjectId = list.ProjectId,
                        ProjectTitle = list.ProjectTitle,
                        ListId = list.Id,
                        ListTitle = list.Title,
                        Members = membersRecords,
                        Labels = labelsRecords                        
                    }, Topics.Tasks, MessageActions.Created);

                return await _tasksRepository.CreateTaskAsync(newTask, taskCollections, outboxMessage);
            }
            catch(Exception)
            {
                //rollback request id
                await _requestsRepository.DeleteRequestIdAsync(requestId);
                throw;
            }            
        }
        

        public async Task<TaskAggregate> UpdateTaskAsync(TaskModel updatingTask, IEnumerable<string> usersIds,
            IEnumerable<string> labelsIds)
        {
            TaskAggregate currentTask = await _tasksRepository.GetTaskAsync(updatingTask.Id);
            if(currentTask == null)
            {
                throw new NotFoundException($"Task with id = {updatingTask.Id} not found");
            }

            if(currentTask.Version != updatingTask.Version)
            {
                throw new VersionsNotMatchException();
            }

            TaskRelatedData taskData = await GetAndCheckTaskRelatedDataAsync(updatingTask, usersIds, labelsIds);

            EnsureProjectNotChanged(currentTask, taskData);

            var (removingCollections, addingCollections) = DetermineAddedAndRemovedItems(currentTask, taskData);
            var list = taskData.List;

            var addedMembersRecords = _mapper.Map<IEnumerable<UserModel>, IEnumerable<TaskUserRecord>>(addingCollections.Users);
            var removedMembersRecords = _mapper.Map<IEnumerable<UserModel>, IEnumerable<TaskUserRecord>>(removingCollections.Users);
            var addedLabelsRecords = _mapper.Map<IEnumerable<LabelModel>, IEnumerable<TaskLabelRecord>>(addingCollections.Labels);
            var removedLabelsRecords = _mapper.Map<IEnumerable<LabelModel>, IEnumerable<TaskLabelRecord>>(removingCollections.Labels);
            
            var outboxMessage = OutboxMessageModel.Create(
                new TaskUpdatedMessage
                {
                    TaskId = updatingTask.Id,
                    Title = updatingTask.Title,
                    ProjectId = list.ProjectId,
                    ProjectTitle = list.ProjectTitle,
                    ListId = list.Id,
                    ListTitle = list.Title,
                    RemovedMembers = removedMembersRecords,
                    AddedMembers = addedMembersRecords,
                    RemovedLabels = removedLabelsRecords,
                    AddedLabels = addedLabelsRecords                        
                }, Topics.Tasks, MessageActions.Updated);


            return await _tasksRepository.UpdateTaskAsync(updatingTask, addingCollections.ToTaskCollections(),
                removingCollections.ToTaskCollections(), outboxMessage);
        }


        public async Task DeleteTaskAsync(string taskId)
        {
            var task = await _tasksRepository.GetSimpleTaskAsync(taskId);
            var outboxMessage = OutboxMessageModel.Create(
                new TaskDeletedMessage
                {
                    TaskId = taskId,
                    Title = task?.Title
                }, Topics.Tasks, MessageActions.Deleted);

            await _tasksRepository.DeleteTaskAsync(taskId, outboxMessage);
        }

        private (TaskRelatedData removingData, TaskRelatedData addingData) DetermineAddedAndRemovedItems(
            TaskAggregate currentTask, TaskRelatedData newData)
        {
            IEnumerable<UserModel> addedMembers = newData.Users.Except<UserModel>(currentTask.Members, _modelsByIdComparer).ToList();
            IEnumerable<LabelModel> addedLabels = newData.Labels.Except<LabelModel>(currentTask.Labels, _modelsByIdComparer).ToList();

            IEnumerable<UserModel> removedMembers = currentTask.Members.Except<UserModel>(newData.Users, _modelsByIdComparer).ToList();
            IEnumerable<LabelModel> removedLabels = currentTask.Labels.Except<LabelModel>(newData.Labels, _modelsByIdComparer).ToList();

            TaskRelatedData addingCollections = new TaskRelatedData
            {
                Users = addedMembers,
                Labels = addedLabels
            };

            TaskRelatedData removingCollections = new TaskRelatedData
            {
                Users = removedMembers,
                Labels = removedLabels
            };

            return (removingCollections, addingCollections);            
        }

        private async Task<TaskRelatedData> GetAndCheckTaskRelatedDataAsync(TaskModel newTask, IEnumerable<string> usersIds, IEnumerable<string> labelsIds)
        {
            Task<ListModelAggregate> listTask = _listsRepository.GetListWithProjectAsync(newTask.ListId);
            await listTask;

            Task<IEnumerable<UserModel>> usersTask = _usersRepository.GetUsersAsync(usersIds);
            await usersTask;

            Task<IEnumerable<LabelModel>> labelsTask = _labelsRepository.GetLabelsAsync(labelsIds);
            await labelsTask;

            Task[] performingTasks = { listTask, usersTask, labelsTask };

            //await Task.WhenAll(performingTasks);

            foreach(var performedTask in performingTasks)
            {
                performedTask.CheckAndThrowIfFaulted();
            }

            var list = listTask.Result;

            await EnsureAllUsersInProject(list.ProjectId, usersIds);

            return new TaskRelatedData
            {
                List = list,
                Users = usersTask.Result,
                Labels = labelsTask.Result
            };
        }

        private void EnsureProjectNotChanged(TaskAggregate currentTask, TaskRelatedData newTaskData)
        {
            if(newTaskData.List.ProjectId != currentTask.ProjectId)
            {
                throw new ProhibitedException($"You can't replace task to list in other project during an update process");
            }
        }

        private async Task EnsureAllUsersInProject(string projectId, IEnumerable<string> usersIds)
        {
            var projectMembers = await _projectMembersRepository.GetProjectMembersAsync(projectId);
            HashSet<string> projectMembersIdsSet = projectMembers.Select(pm => pm.UserId).ToHashSet();
            List<string> notFoundUsers = new List<string>();

            foreach(var userId in usersIds)
            {
                if(!projectMembersIdsSet.Contains(userId))
                {
                    notFoundUsers.Add(userId);
                }
            }

            if(notFoundUsers.Any())
            {
                throw new NotFoundException($"Some users (id = {string.Join(", ", notFoundUsers)}) not found in project with id {projectId}");
            }
        }
    }
}