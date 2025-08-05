using TravelBookingPlatform.Core.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Domain.Entities;

public class RoomType : AggregateRoot
{
    public string Name { get; private set; }
    public string Description { get; private set; } // <-- ADDED
    public decimal PricePerNight { get; private set; }
    public int MaxAdults { get; private set; }
    public int MaxChildren { get; private set; }
    public string? ImageUrl { get; private set; } // <-- ADDED

    // Navigation properties
    public ICollection<Room> Rooms { get; private set; } = new List<Room>();

    // For EF Core
    private RoomType() { }

    public RoomType(string name, string description, decimal pricePerNight, int maxAdults, int maxChildren, string? imageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Room type name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Room type description cannot be empty.", nameof(description));
        if (pricePerNight < 0)
            throw new ArgumentException("Price per night cannot be negative.", nameof(pricePerNight));
        if (maxAdults <= 0)
            throw new ArgumentException("Max adults must be greater than zero.", nameof(maxAdults));
        if (maxChildren < 0)
            throw new ArgumentException("Max children cannot be negative.", nameof(maxChildren));

        Name = name;
        Description = description;
        PricePerNight = pricePerNight;
        MaxAdults = maxAdults;
        MaxChildren = maxChildren;
        ImageUrl = imageUrl;
    }

    public void Update(string name, string description, decimal pricePerNight, int maxAdults, int maxChildren, string? imageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Room type name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Room type description cannot be empty.", nameof(description));
        if (pricePerNight < 0)
            throw new ArgumentException("Price per night cannot be negative.", nameof(pricePerNight));
        if (maxAdults <= 0)
            throw new ArgumentException("Max adults must be greater than zero.", nameof(maxAdults));
        if (maxChildren < 0)
            throw new ArgumentException("Max children cannot be negative.", nameof(maxChildren));

        Name = name;
        Description = description;
        PricePerNight = pricePerNight;
        MaxAdults = maxAdults;
        MaxChildren = maxChildren;
        ImageUrl = imageUrl;
        MarkAsUpdated();
    }

    public bool CanAccommodate(int adults, int children)
    {
        return adults <= MaxAdults && children <= MaxChildren;
    }
}