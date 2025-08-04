using TravelBookingPlatform.Core.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Domain.Entities;

public class Hotel : AggregateRoot
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Rating { get; private set; }
    public string? ImageURL { get; private set; }
    public Guid CityId { get; private set; }

    // Navigation properties
    public City City { get; private set; } = null!;
    public ICollection<Room> Rooms { get; private set; } = new List<Room>();

    // For EF Core
    private Hotel() { }

    public Hotel(string name, string description, decimal rating, Guid cityId, string? imageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Hotel name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.", nameof(description));
        if (rating < 0 || rating > 5)
            throw new ArgumentException("Rating must be between 0 and 5.", nameof(rating));
        if (cityId == Guid.Empty)
            throw new ArgumentException("City ID cannot be empty.", nameof(cityId));

        Name = name;
        Description = description;
        Rating = rating;
        CityId = cityId;
        ImageURL = imageUrl;
    }

    public void Update(string name, string description, decimal rating, string? imageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Hotel name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.", nameof(description));
        if (rating < 0 || rating > 5)
            throw new ArgumentException("Rating must be between 0 and 5.", nameof(rating));

        Name = name;
        Description = description;
        Rating = rating;
        ImageURL = imageUrl;
        MarkAsUpdated();
    }

    public void UpdateCity(Guid cityId)
    {
        if (cityId == Guid.Empty)
            throw new ArgumentException("City ID cannot be empty.", nameof(cityId));

        CityId = cityId;
        MarkAsUpdated();
    }
}