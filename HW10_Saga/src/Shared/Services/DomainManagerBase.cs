using System;
using System.Threading.Tasks;

namespace Shared
{
    public abstract class DomainManagerBase
    {
        private readonly RequestsRepository _requestsRepository;

        public DomainManagerBase(RequestsRepository requestsRepository)
        {
            _requestsRepository = requestsRepository;
        }

        /// <summary>
        /// Check is RequestId was already hanlded and exists in DB
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns>true if the request is new and must be processed</returns>
        protected async Task<bool> CheckAndSaveRequestIdAsync(string requestId)
        {
            bool isRequestAlreadyHadled = await _requestsRepository.IsRequestIdHandledAsync(requestId);
            
            if(isRequestAlreadyHadled)
                return false;

            DateTimeOffset requestIdExpiresAt = DateTimeOffset.UtcNow.AddDays(Constants.RequestIdLifetimeDays);

            await _requestsRepository.SaveRequestIdAsync(requestId, requestIdExpiresAt); 

            return true;          
        }
    }
}