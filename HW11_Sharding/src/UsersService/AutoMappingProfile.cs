using AutoMapper;

namespace UsersService
{
    public class AutoMappingProfile : Profile
    {
        public AutoMappingProfile()
        {  
            CreateMap<CreateUserDto, UserModel>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.Version, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneNumber, opt => opt.Ignore())
                .ForMember(dest => dest.Email, opt => opt.Ignore())
                ;

            CreateMap<UpdateUserDto, UserModel>()
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore());  

            CreateMap<UserModel, UserModel>();
        }
    }
}