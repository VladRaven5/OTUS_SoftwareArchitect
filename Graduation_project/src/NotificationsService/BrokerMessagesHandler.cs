using System;
using System.Collections.Generic;
using AutoMapper;
using Shared;

namespace NotificationsService
{
    public class BrokerMessagesHandler : BrokerMessagesHandlerBase
    {
        protected override List<TopicQueueBindingArgs> _bindingArgs => new List<TopicQueueBindingArgs> 
        { 
            new TopicQueueBindingArgs(Topics.ProjectMembers, "projectmemberstonotifications"),
            new TopicQueueBindingArgs(Topics.Projects, "projectstonotifications"),
            new TopicQueueBindingArgs(Topics.Labels, "labelstonotifications"),
            new TopicQueueBindingArgs(Topics.Tasks, "taskstonotifications"),
        };

        private readonly ProjectsMessageHandler _projectsMessageHandler;
        private readonly ProjectMembersMessageHandler _projectMembersMessageHandler;
        private readonly LabelsMessageHandler _labelsMessageHandler;
        private readonly TasksMessageHandler _tasksMessageHandler;


        private readonly IMapper _mapper;
        private readonly IServiceProvider _serviceProvider;

        public BrokerMessagesHandler(RabbitMqTopicManager rabbitMq, IMapper mapper, IServiceProvider serviceProvider)
            : base(rabbitMq)
        {
            _mapper = mapper;
            _serviceProvider = serviceProvider;

            _projectsMessageHandler = new ProjectsMessageHandler(serviceProvider, mapper);
            _projectMembersMessageHandler = new ProjectMembersMessageHandler(serviceProvider, mapper);
            _labelsMessageHandler = new LabelsMessageHandler(serviceProvider, mapper);
            _tasksMessageHandler = new TasksMessageHandler(serviceProvider, mapper);
        }        

        protected override void OnMessageReceived(ReceivedMessageArgs messageObject)
        {            
            GetHandler(messageObject)
                .HandleMessage(messageObject);
        }

        private DomainMessagesHandlerBase GetHandler(ReceivedMessageArgs messageObject)
        {
            switch(messageObject.Topic)
            {
                case Topics.Projects:                
                    return _projectsMessageHandler;

                case Topics.ProjectMembers:
                    return _projectMembersMessageHandler;

                case Topics.Labels:
                    return _labelsMessageHandler;

                case Topics.Tasks:
                    return _tasksMessageHandler;

                default:
                    throw new NotFoundException($"Topic {messageObject.Topic} is not known");
            }
        }
    }
}