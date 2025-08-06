using MediatR;

namespace TravelBookingPlatform.Modules.Hotels.Application.Notifications;

public class BookingConfirmedNotification : INotification
{
    public Guid BookingId { get; }

    public BookingConfirmedNotification(Guid bookingId)
    {
        BookingId = bookingId;
    }
}