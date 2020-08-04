using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WorkingHoursService
{
    public class TaskUserWorkingHoursService
    {
        private const int _requestIdLifetimeDays = 1;

        private readonly RequestsRepository _requestsRepository;
        private readonly WorkingHoursRepository _workingHoursRepository;

        public TaskUserWorkingHoursService(RequestsRepository requestsRepository,
            WorkingHoursRepository workingHoursRepository)
        {
            _requestsRepository = requestsRepository;
            _workingHoursRepository = workingHoursRepository;
        }

        public Task<IEnumerable<TaskUserWorkingHoursRecord>> GetAllRecordsAsync()
        {
            return _workingHoursRepository.GetWorkingHoursRecordsAsync();
        }

        public Task<TaskUserWorkingHoursRecord> GetRecordByIdAsync(string recordId)
        {
            return _workingHoursRepository.GetWorkingHoursRecordByIdAsync(recordId);
        }

        public async Task<TaskUserWorkingHoursRecord> CreateRecordAsync(TaskUserWorkingHoursRecord newRecord, string requestId)
        {
            if(!(await CheckAndSaveRequestIdAsync(requestId)))
            {
                throw new AlreadyHandledException();
            }

            try
            {
                return await _workingHoursRepository.AddWorkingHoursRecordAsync(newRecord);
            }
            catch(Exception)
            {
                //rollback request id
                await _requestsRepository.DeleteRequestIdAsync(requestId);
                throw;
            }
        }

        public async Task<TaskUserWorkingHoursRecord> UpdateRecordAsync(TaskUserWorkingHoursRecord updatingRecord)
        {
            TaskUserWorkingHoursRecord currentRecord = await _workingHoursRepository.GetWorkingHoursRecordByIdAsync(updatingRecord.Id);
            if(currentRecord == null)
            {
                throw new NotFoundException($"Record with id = {updatingRecord.Id} not found");
            }

            if(currentRecord.Version != updatingRecord.Version)
            {
                throw new VersionsNotMatchException();
            }

            return await _workingHoursRepository.UpdateWorkingHoursRecordAsync(updatingRecord);
        }
        public Task DeleteRecordAsync(string recordId)
        {
            return  _workingHoursRepository.DeleteWorkingHoursRecordAsync(recordId);
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