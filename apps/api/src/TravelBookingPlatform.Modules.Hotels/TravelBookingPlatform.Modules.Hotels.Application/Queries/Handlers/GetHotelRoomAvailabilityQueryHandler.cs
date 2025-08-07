using MediatR;
using TravelBookingPlatform.Core.Domain.Exceptions;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries.Handlers;

public class GetHotelRoomAvailabilityQueryHandler : IRequestHandler<GetHotelRoomAvailabilityQuery, List<AvailableRoomTypeDto>>
{
    private readonly IHotelRepository _hotelRepository;

    public GetHotelRoomAvailabilityQueryHandler(IHotelRepository hotelRepository)
    {
        _hotelRepository = hotelRepository;
    }

    public async Task<List<AvailableRoomTypeDto>> Handle(GetHotelRoomAvailabilityQuery request, CancellationToken cancellationToken)
    {
        var hotel = await _hotelRepository.GetHotelWithRoomsAndBookingsAsync(request.HotelId);

        if (hotel is null)
        {
            throw new NotFoundException(nameof(Hotel), request.HotelId);
        }

        var availableRoomTypes = hotel.Rooms
            .Where(room => room.RoomType.CanAccommodate(request.NumberOfAdults, request.NumberOfChildren))
            .Where(room => room.IsAvailableForPeriod(request.CheckInDate, request.CheckOutDate))
            .GroupBy(room => room.RoomType)
            .Select(group => new AvailableRoomTypeDto
            {
                RoomTypeId = group.Key.Id,
                Name = group.Key.Name,
                Description = group.Key.Description,
                PricePerNight = group.Key.PricePerNight,
                MaxAdults = group.Key.MaxAdults,
                MaxChildren = group.Key.MaxChildren,
                ImageUrl = group.Key.ImageUrl,
                NumberOfAvailableRooms = group.Count()
            })
            .OrderBy(dto => dto.PricePerNight)
            .ToList();

        return availableRoomTypes;
    }
}