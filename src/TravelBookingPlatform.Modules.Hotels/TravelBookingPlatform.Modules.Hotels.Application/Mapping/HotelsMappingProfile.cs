using AutoMapper;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Application.Mapping;

public class HotelsMappingProfile: Profile
{
    public HotelsMappingProfile()
    {
        CreateMap<City, CityDto>();
        // other mappings here
    }
}