using AutoMapper;
using MediatR;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Application.Queries;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries.Handlers;

public class GetHotelByIdQueryHandler : IRequestHandler<GetHotelByIdQuery, HotelDetailDto?>
{
    private readonly IHotelRepository _hotelRepository;
    private readonly IMapper _mapper;

    public GetHotelByIdQueryHandler(IHotelRepository hotelRepository, IMapper mapper)
    {
        _hotelRepository = hotelRepository;
        _mapper = mapper;
    }

    public async Task<HotelDetailDto?> Handle(GetHotelByIdQuery request, CancellationToken cancellationToken)
    {
        var hotel = await _hotelRepository.GetHotelWithDetailsAsync(request.Id);

        if (hotel == null)
            return null;

        var hotelDetailDto = _mapper.Map<HotelDetailDto>(hotel);

        // Calculate summary statistics
        hotelDetailDto.TotalRooms = hotel.Rooms.Count;
        hotelDetailDto.MinPrice = hotel.Rooms.Any() ? hotel.Rooms.Min(r => r.RoomType.PricePerNight) : null;
        hotelDetailDto.MaxPrice = hotel.Rooms.Any() ? hotel.Rooms.Max(r => r.RoomType.PricePerNight) : null;
        hotelDetailDto.AvailableRoomTypes = hotel.Rooms
            .Select(r => r.RoomType.Name)
            .Distinct()
            .ToList();

        return hotelDetailDto;
    }
}