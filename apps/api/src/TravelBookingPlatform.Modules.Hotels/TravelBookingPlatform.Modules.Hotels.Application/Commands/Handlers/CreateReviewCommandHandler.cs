using AutoMapper;
using MediatR;
using TravelBookingPlatform.Core.Domain;
using TravelBookingPlatform.Core.Domain.Exceptions;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

namespace TravelBookingPlatform.Modules.Hotels.Application.Commands.Handlers;

public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, ReviewDto>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IHotelRepository _hotelRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateReviewCommandHandler(
        IBookingRepository bookingRepository,
        IHotelRepository hotelRepository,
        IReviewRepository reviewRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _bookingRepository = bookingRepository;
        _hotelRepository = hotelRepository;
        _reviewRepository = reviewRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ReviewDto> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        // 1. Fetch booking and validate its state and ownership
        var bookings = await _bookingRepository.GetByUserIdAsync(request.UserId);
        var booking = bookings.FirstOrDefault(b => b.Id == request.BookingId);

        if (booking == null)
            throw new NotFoundException("Booking not found or does not belong to the user.", request.BookingId);

        if (booking.CheckOutDate.Date > DateTime.UtcNow.Date)
            throw new BusinessValidationException("You can only review a booking after the check-out date.", "CheckOutDate");

        if (booking.HasBeenReviewed)
            throw new BusinessValidationException("This booking has already been reviewed.", "BookingId");

        var hotel = await _hotelRepository.GetByIdAsync(booking.Room.HotelId);
        if (hotel == null)
            throw new NotFoundException("The hotel associated with this booking could not be found.");

        // 2. Create the Review entity
        var review = new Review(
            hotel.Id,
            request.BookingId,
            request.UserId,
            request.StarRating,
            request.Description);
        await _reviewRepository.AddAsync(review);

        // 3. Mark the booking as reviewed
        booking.MarkAsReviewed();
        _bookingRepository.Update(booking);

        // 4. Recalculate the hotel's average rating
        var allRatings = (await _reviewRepository.GetAllRatingsForHotelAsync(hotel.Id)).ToList();
        allRatings.Add(review.StarRating);
        decimal newAverageRating = (decimal)allRatings.Average();
        hotel.UpdateRating(Math.Round(newAverageRating, 2));
        _hotelRepository.Update(hotel);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ReviewDto>(review);
    }
}