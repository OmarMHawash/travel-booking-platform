using MediatR;

namespace TravelBookingPlatform.Modules.Hotels.Application.Notifications;

public class BookingPaymentFailedNotification : INotification
{
    public Guid BookingId { get; }

    public BookingPaymentFailedNotification(Guid bookingId)
    {
        BookingId = bookingId;
    }
}