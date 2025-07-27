using TravelBookingPlatform.Core.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Domain.Entities;

public class RoomType : AggregateRoot
{
    public string Name { get; private set; }
    public decimal PricePerNight { get; private set; }
    public int MaxAdults { get; private set; }
    public int MaxChildren { get; private set; }

    // Navigation properties
    public ICollection<Room> Rooms { get; private set; } = new List<Room>();

    // For EF Core
    private RoomType() { }

    public RoomType(string name, decimal pricePerNight, int maxAdults, int maxChildren)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Room type name cannot be empty.", nameof(name));
        if (pricePerNight < 0)
            throw new ArgumentException("Price per night cannot be negative.", nameof(pricePerNight));
        if (maxAdults <= 0)
            throw new ArgumentException("Max adults must be greater than zero.", nameof(maxAdults));
        if (maxChildren < 0)
            throw new ArgumentException("Max children cannot be negative.", nameof(maxChildren));

        Name = name;
        PricePerNight = pricePerNight;
        MaxAdults = maxAdults;
        MaxChildren = maxChildren;
    }

    public void Update(string name, decimal pricePerNight, int maxAdults, int maxChildren)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Room type name cannot be empty.", nameof(name));
        if (pricePerNight < 0)
            throw new ArgumentException("Price per night cannot be negative.", nameof(pricePerNight));
        if (maxAdults <= 0)
            throw new ArgumentException("Max adults must be greater than zero.", nameof(maxAdults));
        if (maxChildren < 0)
            throw new ArgumentException("Max children cannot be negative.", nameof(maxChildren));

        Name = name;
        PricePerNight = pricePerNight;
        MaxAdults = maxAdults;
        MaxChildren = maxChildren;
        MarkAsUpdated();
    }

    public bool CanAccommodate(int adults, int children)
    {
        return adults <= MaxAdults && children <= MaxChildren;
    }
}