using TravelBookingPlatform.Core.Domain.Entities;
using TravelBookingPlatform.Modules.Hotels.Domain.Enums;

namespace TravelBookingPlatform.Modules.Hotels.Domain.Entities;

public class Booking : AggregateRoot
{
    public DateTime CheckInDate { get; private set; }
    public DateTime CheckOutDate { get; private set; }
    public Guid RoomId { get; private set; }
    public Guid UserId { get; private set; }
    public bool HasBeenReviewed { get; private set; }
    public BookingStatus Status { get; private set; }
    public decimal TotalPrice { get; private set; }
    public Guid? PaymentId { get; private set; }
    public string GuestName { get; private set; }
    public string? SpecialRequests { get; private set; }
    public string? ConfirmationPdfUrl { get; private set; }
    public bool PdfGenerationFailed { get; private set; }
    public string? PdfGenerationErrorMessage { get; private set; }

    // Navigation properties
    public Room Room { get; private set; } = null!;

    // For EF Core
    private Booking() { }

    // Original constructor - can be kept for seeding or internal use
    public Booking(DateTime checkInDate, DateTime checkOutDate, Guid roomId, Guid userId)
    {
        // Basic constructor logic...
    }

    // New constructor for initiating a booking
    public Booking(
        DateTime checkInDate,
        DateTime checkOutDate,
        Guid roomId,
        Guid userId,
        decimal totalPrice,
        string guestName,
        string? specialRequests)
    {
        if (checkInDate >= checkOutDate)
            throw new ArgumentException("Check-in date must be before check-out date.");
        if (checkInDate.Date < DateTime.Today)
            throw new ArgumentException("Check-in date cannot be in the past.");
        if (roomId == Guid.Empty)
            throw new ArgumentException("Room ID cannot be empty.", nameof(roomId));
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        if (totalPrice <= 0)
            throw new ArgumentException("Total price must be greater than zero.", nameof(totalPrice));
        if (string.IsNullOrWhiteSpace(guestName))
            throw new ArgumentException("Guest name cannot be empty.", nameof(guestName));

        CheckInDate = checkInDate.Date;
        CheckOutDate = checkOutDate.Date;
        RoomId = roomId;
        UserId = userId;
        TotalPrice = totalPrice;
        GuestName = guestName;
        SpecialRequests = specialRequests;
        Status = BookingStatus.PendingPayment;
        HasBeenReviewed = false;
    }

    public void Confirm(Guid paymentId)
    {
        if (Status != BookingStatus.PendingPayment)
            throw new InvalidOperationException("Only a booking pending payment can be confirmed.");

        Status = BookingStatus.Confirmed;
        PaymentId = paymentId;
        MarkAsUpdated();
    }

    public void Cancel()
    {
        if (Status == BookingStatus.Confirmed && CheckInDate <= DateTime.Today.AddDays(2))
            throw new InvalidOperationException("Confirmed bookings cannot be cancelled within 48 hours of check-in.");

        Status = BookingStatus.Cancelled;
        MarkAsUpdated();
    }

    public void MarkAsReviewed()
    {
        if (HasBeenReviewed)
        {
            return;
        }
        HasBeenReviewed = true;
        MarkAsUpdated();
    }

    public int GetNumberOfNights()
    {
        return (CheckOutDate - CheckInDate).Days;
    }

    public void SetConfirmationPdfUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("PDF URL cannot be empty.", nameof(url));

        ConfirmationPdfUrl = url;
        MarkAsUpdated();
    }

    public void MarkPdfGenerationAsFailed(string errorMessage)
    {
        PdfGenerationFailed = true;
        PdfGenerationErrorMessage =
            errorMessage.Length > 1000 ? errorMessage.Substring(0, 1000) : errorMessage;
        MarkAsUpdated();
    }

    public void ResetPdfGenerationStatus()
    {
        PdfGenerationFailed = false;
        PdfGenerationErrorMessage = null;
        MarkAsUpdated();
    }
}