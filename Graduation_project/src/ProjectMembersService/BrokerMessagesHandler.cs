using System;
using System.Collections.Generic;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Shared;

namespace ProjectMembersService
{
    public class BrokerMessagesHandler : BrokerMessagesHandlerBase
    {
        private static List<TopicQueueBindingArgs> _bindingArgs => new List<TopicQueueBindingArgs> 
            { 
                new TopicQueueBindingArgs(Topics.Users, "userstoprojectmembers"),
                new TopicQueueBindingArgs(Topics.Projects, "projectstoprojectmembers"),
            };

        private readonly IMapper _mapper;
        private readonly IServiceProvider _serviceProvider;

        public BrokerMessagesHandler(RabbitMqTopicManager rabbitMq, IMapper mapper, IServiceProvider serviceProvider)
            : base(rabbitMq, _bindingArgs)
        {
            _mapper = mapper;
            _serviceProvider = serviceProvider;
        }        

        protected override void OnMessageReceived(ReceivedMessageArgs messageObject)
        {
            switch(messageObject.Topic)
            {
                case Topics.Users:
                    HandleUserMessage(messageObject);
                    break;

                case Topics.Projects:
                    HandleProjectMessage(messageObject);
                    break;

                default:
                    throw new NotFoundException($"Topic {messageObject.Topic} is not known");
            }
        }

        private void HandleUserMessage(ReceivedMessageArgs messageObject)
        {            
            Console.WriteLine($"User message received");

            switch(messageObject.Action)
            {
                case MessageActions.Created:
                case MessageActions.Updated:
                    var message = JsonConvert.DeserializeObject<UserCreatedUpdatedMessage>(messageObject.Message);
                    var model = _mapper.Map<UserCreatedUpdatedMessage, UserModel>(message);     
                    using(var scope = _serviceProvider.CreateScope())
                        {
                            var requestsRepository = scope.ServiceProvider.GetRequiredService<RequestsRepository>();
                            var usersRepository = scope.ServiceProvider.GetRequiredService<UsersRepository>();
                            if(!requestsRepository.IsHandledOrSaveRequestAsync(message.Id, GetRequestIdInvalidationDate()).GetAwaiter().GetResult())
                            {
                                try
                                {
                                    usersRepository.CreateOrUpdateUserAsync(model).GetAwaiter().GetResult();
                                }
                                catch(Exception)
                                {
                                    requestsRepository.DeleteRequestIdAsync(message.Id).GetAwaiter().GetResult();
                                    throw;
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Already handled: {message.Id}");
                            }
                        }               
                    // PerformSafetyOnBgThread(async () => 
                    // {
                    //     using(var scope = _serviceProvider.CreateScope())
                    //     {
                    //         var requestsRepository = scope.ServiceProvider.GetRequiredService<RequestsRepository>();
                    //         var usersRepository = scope.ServiceProvider.GetRequiredService<UsersRepository>();
                    //         if(!await requestsRepository.IsHandledOrSaveRequestAsync(message.Id, GetRequestIdInvalidationDate()))
                    //         {
                    //             await usersRepository.CreateOrUpdateUserAsync(model);
                    //         }
                    //         else
                    //         {
                    //             Console.WriteLine($"Already handled: {message.Id}");
                    //         }
                    //     }                                         
                    // }, messageObject, TaskScheduler.FromCurrentSynchronizationContext());                    
                break;

                case MessageActions.Deleted:
                    //do nothing for now...
                break;

                default:
                    throw new NotFoundException($"Topic {messageObject.Topic} is not known");                
            }
        }

        private void HandleProjectMessage(ReceivedMessageArgs messageObject)
        {
            Console.WriteLine($"Project message received");
            switch(messageObject.Action)
            {
                case MessageActions.Created:
                case MessageActions.Updated:
                    var message = JsonConvert.DeserializeObject<ProjectCreatedUpdatedMessage>(messageObject.Message);
                    var model = _mapper.Map<ProjectCreatedUpdatedMessage, ProjectModel>(message);     
                    using(var scope = _serviceProvider.CreateScope())
                        {
                            var requestsRepository = scope.ServiceProvider.GetRequiredService<RequestsRepository>();
                            var projectsRepository = scope.ServiceProvider.GetRequiredService<ProjectsRepository>();
                            if(!requestsRepository.IsHandledOrSaveRequestAsync(message.Id, GetRequestIdInvalidationDate()).GetAwaiter().GetResult())
                            {
                                try
                                {
                                    projectsRepository.CreateOrUpdateProjectAsync(model).GetAwaiter().GetResult();
                                }
                                catch(Exception)
                                {
                                    requestsRepository.DeleteRequestIdAsync(message.Id).GetAwaiter().GetResult();
                                    throw;
                                }                                
                            }
                            else
                            {
                                Console.WriteLine($"Already handled: {message.Id}");
                            }
                        }                    
                break;

                case MessageActions.Deleted:
                    //do nothing for now...
                break;

                default:
                    throw new NotFoundException($"Topic {messageObject.Topic} is not known");
            }
        }
    }
}