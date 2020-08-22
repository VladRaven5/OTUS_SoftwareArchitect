using AutoMapper;
using Shared;

namespace ListsService
{
    public class AutoMappingProfile : Profile
    {
        public AutoMappingProfile()
        {
            CreateMap<CreateListDto, ListModel>()
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Version, opt => opt.Ignore());

            CreateMap<UpdateListDto, ListModel>()
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectId, opt => opt.Ignore());

            CreateMap<ProjectCreatedUpdatedMessage, ProjectModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ProjectId));        
        }
    }
}