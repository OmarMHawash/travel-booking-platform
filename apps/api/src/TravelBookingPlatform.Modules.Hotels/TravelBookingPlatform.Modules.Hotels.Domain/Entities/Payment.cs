using TravelBookingPlatform.Core.Domain.Entities;
using TravelBookingPlatform.Modules.Hotels.Domain.Enums;

namespace TravelBookingPlatform.Modules.Hotels.Domain.Entities;

public class Payment : AggregateRoot
{
    public Guid BookingId { get; private set; }
    public decimal Amount { get; private set; }
    public string StripePaymentIntentId { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string PaymentMethod { get; private set; }

    // Navigation Property
    public Booking Booking { get; private set; } = null!;

    private Payment() { } // For EF Core

    public Payment(Guid bookingId, decimal amount, string stripePaymentIntentId, string paymentMethod)
    {
        if (bookingId == Guid.Empty)
            throw new ArgumentException("Booking ID cannot be empty.", nameof(bookingId));
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.", nameof(amount));
        if (string.IsNullOrWhiteSpace(stripePaymentIntentId))
            throw new ArgumentException("Stripe Payment Intent ID cannot be empty.", nameof(stripePaymentIntentId));

        BookingId = bookingId;
        Amount = amount;
        StripePaymentIntentId = stripePaymentIntentId;
        PaymentMethod = paymentMethod;
        Status = PaymentStatus.Succeeded;
    }
}