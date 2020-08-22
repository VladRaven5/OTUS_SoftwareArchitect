using AutoMapper;
using Shared;

namespace TasksService
{
    public class AutoMappingProfile : Profile
    {
        public AutoMappingProfile()
        {
            CreateMap<CreateTaskDto, TaskModel>()
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Version, opt => opt.Ignore());

            CreateMap<UpdateTaskDto, TaskModel>()
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore());     

            CreateMap<TaskQueryJoinedResult, TaskAggregate>()
                .ForMember(dest => dest.Members, opt => opt.Ignore())
                .ForMember(dest => dest.Labels, opt => opt.Ignore());  

            CreateMap<TaskQueryJoinedResult, UserModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId));

            CreateMap<TaskQueryJoinedResult, LabelModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.LabelId))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.LabelTitle))
                .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.LabelColor)); 

            CreateMap<UserModel, TaskUserRecord>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id));

            CreateMap<LabelModel, TaskLabelRecord>()
                .ForMember(dest => dest.LabelId, opt => opt.MapFrom(src => src.Id));
                
            

            CreateMap<ProjectCreatedUpdatedMessage, ProjectModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ProjectId));            
            
            CreateMap<UserCreatedUpdatedMessage, UserModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId));

            CreateMap<ProjectMemberCreatedUpdatedMessage, ProjectMemberModel>();

            CreateMap<ProjectMemberDeletedMessage, ProjectMemberModel>();

            CreateMap<LabelCreatedUpdatedMessage, LabelModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.LabelId));

            CreateMap<ListCreatedMessage, ListModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ListId));

            CreateMap<ListUpdatedMessage, ListModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ListId))
                .ForMember(dest => dest.ProjectId, opt => opt.Ignore());            
        }
    }
}