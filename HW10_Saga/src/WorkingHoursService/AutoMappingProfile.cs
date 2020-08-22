using AutoMapper;
using Shared;

namespace WorkingHoursService
{
    public class AutoMappingProfile : Profile
    {
        public AutoMappingProfile()
        {
            CreateMap<CreateWorkingHoursRecordDto, TaskUserWorkingHoursRecord>()
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Version, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore());

            CreateMap<UpdateWorkingHoursRecordDto, TaskUserWorkingHoursRecord>()
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore());
                

            CreateMap<ProjectCreatedUpdatedMessage, ProjectModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ProjectId));
            
            CreateMap<UserCreatedUpdatedMessage, UserModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId));  

            CreateMap<TaskCreatedMessage, TaskModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.TaskId));

            CreateMap<TaskUpdatedMessage, TaskModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.TaskId));

        }
    }
}