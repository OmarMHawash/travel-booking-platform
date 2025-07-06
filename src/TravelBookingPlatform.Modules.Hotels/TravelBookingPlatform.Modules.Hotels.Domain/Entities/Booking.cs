using TravelBookingPlatform.Core.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Domain.Entities;

public class Booking : AggregateRoot
{
    public DateTime CheckInDate { get; private set; }
    public DateTime CheckOutDate { get; private set; }
    public Guid RoomId { get; private set; }
    public Guid UserId { get; private set; } // Loose coupling - no navigation property

    // Navigation properties
    public Room Room { get; private set; } = null!;

    // For EF Core
    private Booking() { }

    public Booking(DateTime checkInDate, DateTime checkOutDate, Guid roomId, Guid userId)
    {
        if (checkInDate >= checkOutDate)
            throw new ArgumentException("Check-in date must be before check-out date.");
        if (checkInDate.Date < DateTime.Today)
            throw new ArgumentException("Check-in date cannot be in the past.");
        if (roomId == Guid.Empty)
            throw new ArgumentException("Room ID cannot be empty.", nameof(roomId));
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        CheckInDate = checkInDate.Date; // Ensure we only store the date part
        CheckOutDate = checkOutDate.Date;
        RoomId = roomId;
        UserId = userId;
    }

    public void Update(DateTime checkInDate, DateTime checkOutDate)
    {
        if (checkInDate >= checkOutDate)
            throw new ArgumentException("Check-in date must be before check-out date.");
        if (checkInDate.Date < DateTime.Today)
            throw new ArgumentException("Check-in date cannot be in the past.");

        CheckInDate = checkInDate.Date;
        CheckOutDate = checkOutDate.Date;
        MarkAsUpdated();
    }

    public int GetNumberOfNights()
    {
        return (CheckOutDate - CheckInDate).Days;
    }

    public bool OverlapsWith(DateTime otherCheckIn, DateTime otherCheckOut)
    {
        return CheckInDate < otherCheckOut && CheckOutDate > otherCheckIn;
    }

    public bool OverlapsWith(Booking otherBooking)
    {
        return OverlapsWith(otherBooking.CheckInDate, otherBooking.CheckOutDate);
    }
}