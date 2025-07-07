using AutoMapper;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Application.Mapping;

public class HotelsMappingProfile : Profile
{
    public HotelsMappingProfile()
    {
        CreateMap<City, CityDto>();

        // Deal mappings
        CreateMap<Deal, FeaturedDealDto>()
            .ForMember(dest => dest.Hotel, opt => opt.MapFrom(src => new HotelSummaryDto
            {
                Id = src.Hotel.Id,
                Name = src.Hotel.Name,
                Rating = src.Hotel.Rating,
                City = src.Hotel.City.Name,
                Country = src.Hotel.City.Country,
                ImageURL = src.Hotel.ImageURL
            }))
            .ForMember(dest => dest.RoomType, opt => opt.MapFrom(src => src.RoomType != null ? new RoomTypeSummaryDto
            {
                Id = src.RoomType.Id,
                Name = src.RoomType.Name,
                MaxAdults = src.RoomType.MaxAdults,
                MaxChildren = src.RoomType.MaxChildren
            } : null));

        CreateMap<Deal, DealDto>()
            .ForMember(dest => dest.Hotel, opt => opt.MapFrom(src => new HotelSummaryDto
            {
                Id = src.Hotel.Id,
                Name = src.Hotel.Name,
                Rating = src.Hotel.Rating,
                City = src.Hotel.City.Name,
                Country = src.Hotel.City.Country,
                ImageURL = src.Hotel.ImageURL
            }))
            .ForMember(dest => dest.RoomType, opt => opt.MapFrom(src => src.RoomType != null ? new RoomTypeSummaryDto
            {
                Id = src.RoomType.Id,
                Name = src.RoomType.Name,
                MaxAdults = src.RoomType.MaxAdults,
                MaxChildren = src.RoomType.MaxChildren
            } : null));

        // other mappings here
    }
}