using AutoMapper;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Application.Commands;
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
            }))
            .ForMember(dest => dest.RoomType, opt => opt.MapFrom(src => src.RoomType != null ? new RoomTypeSummaryDto
            {
                Id = src.RoomType.Id,
                Name = src.RoomType.Name,
                MaxAdults = src.RoomType.MaxAdults,
                MaxChildren = src.RoomType.MaxChildren
            } : null));

        // Hotel detail mappings
        CreateMap<Hotel, HotelDetailDto>()
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images.OrderBy(i => i.SortOrder).ToList()));

        CreateMap<Room, RoomDetailDto>();

        CreateMap<RoomType, RoomTypeDetailDto>();

        CreateMap<CreateBookingRequestDto, CreateBookingCommand>();

        CreateMap<Booking, UserBookingDto>()
    .ForMember(dest => dest.BookingId, opt => opt.MapFrom(src => src.Id))
    .ForMember(dest => dest.ConfirmationNumber, opt => opt.MapFrom(src => $"BKG-{src.Id.ToString().Substring(0, 8).ToUpper()}"))
    .ForMember(dest => dest.HotelId, opt => opt.MapFrom(src => src.Room.Hotel.Id))
    .ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.Room.Hotel.Name))
    .ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.Room.Hotel.City.Name))
    .ForMember(dest => dest.RoomTypeName, opt => opt.MapFrom(src => src.Room.RoomType.Name))
    .ForMember(dest => dest.TotalNights, opt => opt.MapFrom(src => src.GetNumberOfNights()))
    .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.GetNumberOfNights() * src.Room.RoomType.PricePerNight))
    .ForMember(dest => dest.BookedAt, opt => opt.MapFrom(src => src.CreatedAt))
    .ForMember(dest => dest.BookingStatus, opt => opt.MapFrom(src =>
        src.CheckOutDate.Date < DateTime.Today ? "Completed"
        : src.CheckInDate.Date > DateTime.Today ? "Upcoming"
        : "In Progress"));

        CreateMap<HotelImage, HotelImageDto>();

        CreateMap<Review, ReviewDto>()
            .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src => src.UserId));
    }


}