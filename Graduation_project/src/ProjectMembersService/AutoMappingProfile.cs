using AutoMapper;
using Shared;

namespace ProjectMembersService
{
    public class AutoMappingProfile : Profile
    {
        public AutoMappingProfile()
        {
            CreateMap<ProjectMemberDto, ProjectMemberModel>();
            
            CreateMap<ProjectCreatedUpdatedMessage, ProjectModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ProjectId));
            
            CreateMap<UserCreatedUpdatedMessage, UserModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId));       
        }
    }
}