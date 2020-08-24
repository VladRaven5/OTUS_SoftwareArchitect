using System;
using System.Collections.Generic;
using AutoMapper;
using Shared;

namespace TasksService
{
    public class BrokerMessagesHandler : BrokerMessagesHandlerBase
    {
        protected override List<TopicQueueBindingArgs> _bindingArgs => new List<TopicQueueBindingArgs> 
        { 
            new TopicQueueBindingArgs(Topics.Users, "userstotasks"),
            new TopicQueueBindingArgs(Topics.ProjectMembers, "projectmemberstotasks"),
            new TopicQueueBindingArgs(Topics.Projects, "projectstotasks"),
            new TopicQueueBindingArgs(Topics.Labels, "labelstotasks"),
            new TopicQueueBindingArgs(Topics.Lists, "liststotasks"),
            new TopicQueueBindingArgs(Topics.WorkingHours, "workinghourstotasks"),
        };

        private readonly UsersMessageHandler _usersMessagesHandler;
        private readonly ProjectsMessageHandler _projectsMessageHandler;
        private readonly ProjectMembersMessageHandler _projectMembersMessageHandler;
        private readonly ListsMessageHandler _listsMessageHandler;
        private readonly LabelsMessageHandler _labelsMessageHandler;
        private readonly TransactionMessagesHandler _transactionHanlder;


        private readonly IMapper _mapper;
        private readonly IServiceProvider _serviceProvider;

        public BrokerMessagesHandler(RabbitMqTopicManager rabbitMq, IMapper mapper, IServiceProvider serviceProvider)
            : base(rabbitMq)
        {
            _mapper = mapper;
            _serviceProvider = serviceProvider;

            _usersMessagesHandler = new UsersMessageHandler(serviceProvider, mapper);
            _projectsMessageHandler = new ProjectsMessageHandler(serviceProvider, mapper);
            _projectMembersMessageHandler = new ProjectMembersMessageHandler(serviceProvider, mapper);
            _listsMessageHandler = new ListsMessageHandler(serviceProvider, mapper);
            _labelsMessageHandler = new LabelsMessageHandler(serviceProvider, mapper);
            _transactionHanlder = new TransactionMessagesHandler(serviceProvider, mapper);
        }        

        protected override void OnMessageReceived(ReceivedMessageArgs messageObject)
        {            
            GetHandler(messageObject)
                .HandleMessage(messageObject);
        }

        private DomainMessagesHandlerBase GetHandler(ReceivedMessageArgs messageObject)
        {
            if(TransactionMessagesHandler.Actions.Contains(messageObject.Action))
                return _transactionHanlder;


            switch(messageObject.Topic)
            {
                case Topics.Users:
                    return _usersMessagesHandler;

                case Topics.Projects:                
                    return _projectsMessageHandler;

                case Topics.ProjectMembers:
                    return _projectMembersMessageHandler;

                case Topics.Labels:
                    return _labelsMessageHandler;

                case Topics.Lists:
                    return _listsMessageHandler;

                default:
                    throw new NotFoundException($"Topic {messageObject.Topic} is not known");
            }
        }
    }
}