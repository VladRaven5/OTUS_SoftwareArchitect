using System;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Shared;

namespace NotificationsService
{
    public abstract class MessagesNotificationHandlerBase<TRepository>
        : DomainMessagesHandlerBase<TRepository> where TRepository : BaseDapperRepository
    {
        public MessagesNotificationHandlerBase(IServiceProvider serviceProvider, IMapper mapper)
            : base(serviceProvider, mapper)
        {
        }

        protected abstract void HandleNotificationInternal(BaseMessage message, string action, TRepository repository,
            NotificationsRepository notificationsRepository);

        protected override void HandleMessageInternal(BaseMessage message, string action, TRepository repository)
        {
            using(var scope = _serviceProvider.CreateScope())
            {
                var notificationsRepository = scope.ServiceProvider.GetRequiredService<NotificationsRepository>();

                HandleNotificationInternal(message, action, repository, notificationsRepository);
            }            
        }
    }
}