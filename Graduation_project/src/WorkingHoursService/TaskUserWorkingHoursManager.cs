using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shared;

namespace WorkingHoursService
{
    public class TaskUserWorkingHoursManager
    {
        private readonly int _requestIdLifetimeDays = Constants.RequestIdLifetimeDays;

        private readonly RequestsRepository _requestsRepository;
        private readonly WorkingHoursRepository _workingHoursRepository;
        private readonly TasksRepository _tasksRepository;
        private readonly UsersRepository _usersRepository;

        public TaskUserWorkingHoursManager(RequestsRepository requestsRepository,
            WorkingHoursRepository workingHoursRepository, TasksRepository tasksRepository,
            UsersRepository usersRepository)
        {
            _requestsRepository = requestsRepository;
            _workingHoursRepository = workingHoursRepository;
            _tasksRepository = tasksRepository;
            _usersRepository = usersRepository;
        }

        public Task<IEnumerable<MemberWorkingHoursAggregate>> GetProjectWorkingHoursAsync(string projectId, string taskId, string userId)
        {
            return _workingHoursRepository.GetProjectTaskMemberWorkingHoursAsync(projectId, taskId, userId);
        }

        public async Task<TaskUserWorkingHoursRecord> GetRecordByIdAsync(string recordId)
        {
            var record = await _workingHoursRepository.GetWorkingHoursRecordByIdAsync(recordId);
            if(record == null)
            {
                throw new NotFoundException($"Record with id = {recordId} not found");
            }

            return record;
        }

        public async Task<TaskUserWorkingHoursRecord> CreateRecordAsync(TaskUserWorkingHoursRecord newRecord, string requestId)
        {
            if(!(await CheckAndSaveRequestIdAsync(requestId)))
            {
                throw new AlreadyHandledException();
            }

            try
            {
                await EnsureTaskUserExistAsync(newRecord.TaskId, newRecord.UserId);

                return await _workingHoursRepository.AddWorkingHoursRecordAsync(newRecord);
            }
            catch(Exception)
            {
                //rollback request id
                await _requestsRepository.DeleteRequestIdAsync(requestId);
                throw;
            }
        }        

        public Task<IEnumerable<MemberWorkingHoursAggregate>> GetUserHoursAsync(string userId)
        {
            return _workingHoursRepository.GetProjectTaskMemberWorkingHoursAsync(userId: userId);
        }

        public async Task<TaskUserWorkingHoursRecord> UpdateRecordAsync(TaskUserWorkingHoursRecord updatingRecord)
        {
            TaskUserWorkingHoursRecord currentRecord = await _workingHoursRepository.GetWorkingHoursRecordByIdAsync(updatingRecord.Id);
       
            if(currentRecord.Version != updatingRecord.Version)
            {
                throw new VersionsNotMatchException();
            }

            await EnsureTaskUserExistAsync(updatingRecord.TaskId, updatingRecord.UserId);

            return await _workingHoursRepository.UpdateWorkingHoursRecordAsync(updatingRecord);
        }
        public Task DeleteRecordAsync(string recordId)
        {
            return _workingHoursRepository.DeleteWorkingHoursRecordAsync(recordId);
        }

        private async Task EnsureTaskUserExistAsync(string taskId, string userId)
        {
            Task<TaskModel> taskTask = _tasksRepository.GetTaskAsync(taskId);
            Task<UserModel> userTask = _usersRepository.GetUserAsync(userId);

            await Task.WhenAll(taskTask, userTask);

            if(taskTask.Result == null)
            {
                throw new NotFoundException($"Task with id = {taskId} not found");
            }

            if(userTask.Result == null)
            {
                throw new NotFoundException($"User with id = {userId} not found");
            }
        }

        private async Task<bool> CheckAndSaveRequestIdAsync(string requestId)
        {
            bool isRequestAlreadyHadled = await _requestsRepository.IsRequestIdHandledAsync(requestId);
            
            if(isRequestAlreadyHadled)
                return false;

            DateTimeOffset requestIdExpiresAt = DateTimeOffset.UtcNow.AddDays(_requestIdLifetimeDays);

            await _requestsRepository.SaveRequestIdAsync(requestId, requestIdExpiresAt); 

            return true;          
        }
    }
}