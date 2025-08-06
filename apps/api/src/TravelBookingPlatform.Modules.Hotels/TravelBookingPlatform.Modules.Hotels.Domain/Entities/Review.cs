using TravelBookingPlatform.Core.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Domain.Entities;

public class Review : AggregateRoot
{
    public Guid HotelId { get; private set; }
    public Guid BookingId { get; private set; }
    public Guid UserId { get; private set; }
    public int StarRating { get; private set; }
    public string Description { get; private set; }

    // Navigation properties for context, but not for direct manipulation
    public Hotel Hotel { get; private set; } = null!;
    public Booking Booking { get; private set; } = null!;

    // For EF Core
    private Review() { }

    public Review(Guid hotelId, Guid bookingId, Guid userId, int starRating, string description)
    {
        if (hotelId == Guid.Empty)
            throw new ArgumentException("Hotel ID cannot be empty.", nameof(hotelId));
        if (bookingId == Guid.Empty)
            throw new ArgumentException("Booking ID cannot be empty.", nameof(bookingId));
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        if (starRating < 1 || starRating > 5)
            throw new ArgumentException("Star rating must be between 1 and 5.", nameof(starRating));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Review description cannot be empty.", nameof(description));

        HotelId = hotelId;
        BookingId = bookingId;
        UserId = userId;
        StarRating = starRating;
        Description = description;
    }
}