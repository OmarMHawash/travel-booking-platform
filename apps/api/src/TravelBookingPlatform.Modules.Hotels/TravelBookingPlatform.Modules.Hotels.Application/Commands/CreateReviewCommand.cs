using MediatR;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;

namespace TravelBookingPlatform.Modules.Hotels.Application.Commands;

public class CreateReviewCommand : IRequest<ReviewDto>
{
    public Guid BookingId { get; }
    public Guid UserId { get; }
    public int StarRating { get; }
    public string Description { get; }

    public CreateReviewCommand(Guid bookingId, Guid userId, int starRating, string description)
    {
        BookingId = bookingId;
        UserId = userId;
        StarRating = starRating;
        Description = description;
    }
}