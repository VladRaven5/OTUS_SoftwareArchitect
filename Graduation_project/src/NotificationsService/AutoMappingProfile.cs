using AutoMapper;
using Shared;

namespace NotificationsService
{
    public class AutoMappingProfile : Profile
    {
        public AutoMappingProfile()
        {
            CreateMap<ProjectMemberCreatedUpdatedMessage, ProjectMemberModel>();
            CreateMap<ProjectMemberDeletedMessage, ProjectMemberModel>();          
        }
    }
}