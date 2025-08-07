using MediatR;
using TravelBookingPlatform.Core.Domain;
using TravelBookingPlatform.Core.Domain.Exceptions;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

namespace TravelBookingPlatform.Modules.Hotels.Application.Commands.Handlers;

public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, BookingConfirmationDto>
{
    private readonly IHotelRepository _hotelRepository;
    private readonly IRoomTypeRepository _roomTypeRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBookingCommandHandler(
        IHotelRepository hotelRepository,
        IRoomTypeRepository roomTypeRepository,
        IRoomRepository roomRepository,
        IBookingRepository bookingRepository,
        IUnitOfWork unitOfWork)
    {
        _hotelRepository = hotelRepository;
        _roomTypeRepository = roomTypeRepository;
        _roomRepository = roomRepository;
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BookingConfirmationDto> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate existence of related entities
        var hotel = await _hotelRepository.GetByIdAsync(request.HotelId)
            ?? throw new NotFoundException(nameof(Hotel), request.HotelId);

        var roomType = await _roomTypeRepository.GetByIdAsync(request.RoomTypeId)
            ?? throw new NotFoundException(nameof(RoomType), request.RoomTypeId);

        // 2. Validate business rules
        if (!roomType.CanAccommodate(request.NumberOfAdults, request.NumberOfChildren))
        {
            throw new BusinessValidationException("The selected room type cannot accommodate the specified number of guests.", "NumberOfAdults");
        }

        // 3. Check for availability (This is the critical race-condition check)
        var availableRooms = await _roomRepository.GetAvailableRoomsByTypeForPeriodAsync(
            request.HotelId, request.RoomTypeId, request.CheckInDate, request.CheckOutDate);

        if (availableRooms.Count < request.NumberOfRooms)
        {
            throw new BusinessValidationException("The selected room type is no longer available for the chosen dates. Please try again.", "RoomTypeId");
        }

        // 4. Create Booking(s) in a transaction
        Booking newBooking;
        try
        {
            var roomToBook = availableRooms.First(); // For this simple case, we book the first available room.
                                                     // For booking multiple rooms, we would iterate 'request.NumberOfRooms' times.

            newBooking = new Booking(
                request.CheckInDate,
                request.CheckOutDate,
                roomToBook.Id,
                request.UserId);

            await _bookingRepository.AddAsync(newBooking);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // The Unit of Work will handle the rollback.
            throw new Exception("Failed to save the booking. Please try again.", ex);
        }

        // 5. Return confirmation DTO
        var totalNights = newBooking.GetNumberOfNights();
        return new BookingConfirmationDto
        {
            BookingId = newBooking.Id,
            ConfirmationNumber = $"BKG-{newBooking.Id.ToString().ToUpper().Split('-').First()}",
            HotelName = hotel.Name,
            RoomTypeName = roomType.Name,
            CheckInDate = newBooking.CheckInDate,
            CheckOutDate = newBooking.CheckOutDate,
            TotalNights = totalNights,
            TotalPrice = totalNights * roomType.PricePerNight
        };
    }
}