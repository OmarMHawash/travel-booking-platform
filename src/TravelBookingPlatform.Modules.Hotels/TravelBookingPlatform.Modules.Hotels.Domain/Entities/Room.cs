using TravelBookingPlatform.Core.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Domain.Entities;

public class Room : AggregateRoot
{
    public string RoomNumber { get; private set; }
    public Guid HotelId { get; private set; }
    public Guid RoomTypeId { get; private set; }

    // Navigation properties
    public Hotel Hotel { get; private set; } = null!;
    public RoomType RoomType { get; private set; } = null!;
    public ICollection<Booking> Bookings { get; private set; } = new List<Booking>();

    // For EF Core
    private Room() { }

    public Room(string roomNumber, Guid hotelId, Guid roomTypeId)
    {
        if (string.IsNullOrWhiteSpace(roomNumber))
            throw new ArgumentException("Room number cannot be empty.", nameof(roomNumber));
        if (hotelId == Guid.Empty)
            throw new ArgumentException("Hotel ID cannot be empty.", nameof(hotelId));
        if (roomTypeId == Guid.Empty)
            throw new ArgumentException("Room type ID cannot be empty.", nameof(roomTypeId));

        RoomNumber = roomNumber;
        HotelId = hotelId;
        RoomTypeId = roomTypeId;
    }

    public void Update(string roomNumber, Guid roomTypeId)
    {
        if (string.IsNullOrWhiteSpace(roomNumber))
            throw new ArgumentException("Room number cannot be empty.", nameof(roomNumber));
        if (roomTypeId == Guid.Empty)
            throw new ArgumentException("Room type ID cannot be empty.", nameof(roomTypeId));

        RoomNumber = roomNumber;
        RoomTypeId = roomTypeId;
        MarkAsUpdated();
    }

    public bool IsAvailableForPeriod(DateTime checkInDate, DateTime checkOutDate)
    {
        if (checkInDate >= checkOutDate)
            throw new ArgumentException("Check-in date must be before check-out date.");

        return !Bookings.Any(booking =>
            booking.CheckInDate < checkOutDate && booking.CheckOutDate > checkInDate);
    }
}