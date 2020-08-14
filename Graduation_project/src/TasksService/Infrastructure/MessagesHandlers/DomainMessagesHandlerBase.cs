using System;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Shared;

namespace TasksService
{
    public abstract class DomainMessagesHandlerBase<TRepository> : DomainMessagesHandlerBase where TRepository : BaseDapperRepository
    {
        private readonly int _brokerMessagesLifetimeDays = 2;
        protected readonly IServiceProvider _serviceProvider;
        protected readonly IMapper _mapper;

        protected abstract string _identifier { get; }

        public DomainMessagesHandlerBase(IServiceProvider serviceProvider, IMapper mapper)
        {
            _serviceProvider = serviceProvider;
            _mapper = mapper;
        }

        public override void HandleMessage(ReceivedMessageArgs messageObject)
        {
            Console.WriteLine($"{_identifier} message received"); 

            using(var scope = _serviceProvider.CreateScope())
            {
                Type messageType = GetMessageTypeForAction(messageObject.Action);
                var message = DeserializeMessage(messageObject, messageType);

                var requestsRepository = scope.ServiceProvider.GetRequiredService<RequestsRepository>();
                var repository = scope.ServiceProvider.GetRequiredService<TRepository>();
                
                if(CheckIsHandleRequired(message, requestsRepository))
                {
                    try
                    {
                        HandleMessageInternal(message, messageObject.Action, repository);                        
                    }
                    catch(Exception)
                    {
                        requestsRepository.DeleteRequestIdAsync(message.Id).GetAwaiter().GetResult();
                        throw;
                    }
                }
                else
                {
                    Console.WriteLine($"{_identifier} message already handled: {message.Id}");
                }
            }
        }

        private bool CheckIsHandleRequired(BaseMessage message, RequestsRepository requestsRepository)
        {
            return !requestsRepository.IsHandledOrSaveRequestAsync(
                    message.Id,
                    GetRequestIdInvalidationDate())
                .GetAwaiter()
                .GetResult();
        }

        protected abstract void HandleMessageInternal(BaseMessage message, string action, TRepository repository);

        protected virtual BaseMessage DeserializeMessage(ReceivedMessageArgs messageObject, Type type)
        {
            return JsonConvert.DeserializeObject(messageObject.Message, type) as BaseMessage;
        }

        protected abstract Type GetMessageTypeForAction(string action); 

        protected virtual DateTimeOffset GetRequestIdInvalidationDate()
        {
            return DateTimeOffset.UtcNow.AddDays(_brokerMessagesLifetimeDays);
        }

        protected void ThrowUnknownMessageException(BaseMessage message)
        {
            throw new NotFoundException($"Unknonw {_identifier} message type: {message.GetType().Name}");
        }
    }

    public abstract class DomainMessagesHandlerBase
    {
        public abstract void HandleMessage(ReceivedMessageArgs messageObject);
    }
}