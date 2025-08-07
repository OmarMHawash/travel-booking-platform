using MediatR;
using TravelBookingPlatform.Core.Domain;
using TravelBookingPlatform.Core.Domain.Exceptions;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Application.Interfaces;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

namespace TravelBookingPlatform.Modules.Hotels.Application.Commands.Handlers;

public class InitiateBookingCommandHandler : IRequestHandler<InitiateBookingCommand, InitiateBookingResponseDto>
{
    private readonly IHotelRepository _hotelRepository;
    private readonly IRoomTypeRepository _roomTypeRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentGatewayService _paymentGatewayService; // Will be mocked for now

    public InitiateBookingCommandHandler(
        IHotelRepository hotelRepository,
        IRoomTypeRepository roomTypeRepository,
        IRoomRepository roomRepository,
        IBookingRepository bookingRepository,
        IUnitOfWork unitOfWork,
        IPaymentGatewayService paymentGatewayService)
    {
        _hotelRepository = hotelRepository;
        _roomTypeRepository = roomTypeRepository;
        _roomRepository = roomRepository;
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
        _paymentGatewayService = paymentGatewayService;
    }

    public async Task<InitiateBookingResponseDto> Handle(InitiateBookingCommand request, CancellationToken cancellationToken)
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

        var availableRooms = await _roomRepository.GetAvailableRoomsByTypeForPeriodAsync(
            request.HotelId, request.RoomTypeId, request.CheckInDate, request.CheckOutDate);

        if (availableRooms.Count < 1)
        {
            throw new BusinessValidationException("The selected room type is no longer available for the chosen dates. Please try again.", "RoomTypeId");
        }
        var roomToBook = availableRooms.First();

        // 3. Calculate Total Price
        var numberOfNights = (request.CheckOutDate - request.CheckInDate).Days;
        var totalPrice = numberOfNights * roomType.PricePerNight;

        // 4. Create Pending Booking
        var newBooking = new Booking(
            request.CheckInDate,
            request.CheckOutDate,
            roomToBook.Id,
            request.UserId,
            totalPrice,
            request.GuestName,
            request.SpecialRequests
        );

        await _bookingRepository.AddAsync(newBooking);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 5. Create Payment Intent with the payment gateway
        // This will be mocked in tests and a dummy implementation for now.
        var clientSecret = await _paymentGatewayService.CreatePaymentIntentAsync(totalPrice, "usd", newBooking.Id);

        // 6. Return response to the client
        return new InitiateBookingResponseDto
        {
            BookingId = newBooking.Id,
            ClientSecret = clientSecret
        };
    }
}