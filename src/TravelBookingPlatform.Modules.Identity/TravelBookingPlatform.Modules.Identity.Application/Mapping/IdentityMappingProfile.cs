using AutoMapper;
using TravelBookingPlatform.Modules.Identity.Application.DTOs;
using TravelBookingPlatform.Modules.Identity.Domain.Entities;

namespace TravelBookingPlatform.Modules.Identity.Application.Mapping;

public class IdentityMappingProfile : Profile
{
    public IdentityMappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value));

        CreateMap<RegisterUserDto, User>();
    }
}