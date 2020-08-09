using AutoMapper;

namespace LabelsService
{
    public class AutoMappingProfile : Profile
    {
        public AutoMappingProfile()
        {
            CreateMap<CreateLabelDto, LabelModel>()
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Version, opt => opt.Ignore());

            CreateMap<UpdateLabelDto, LabelModel>()
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore());
        }
    }
}