using AutoMapper;

namespace WorkingHoursService
{
    public class AutoMappingProfile : Profile
    {
        public AutoMappingProfile()
        {
            CreateMap<CreateWorkingHoursRecordDto, TaskUserWorkingHoursRecord>()
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Version, opt => opt.Ignore());

            CreateMap<UpdateWorkingHoursRecordDto, TaskUserWorkingHoursRecord>()
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore());
        }
    }
}