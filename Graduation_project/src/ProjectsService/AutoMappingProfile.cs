using AutoMapper;

namespace ProjectsService
{
    public class AutoMappingProfile : Profile
    {
        public AutoMappingProfile()
        {
            CreateMap<CreateProjectDto, ProjectModel>()
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Version, opt => opt.Ignore());

            CreateMap<UpdateProjectDto, ProjectModel>()
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore());
        }
    }
}