using TravelBookingPlatform.Core.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Domain.Entities;

public class Hotel : AggregateRoot
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Rating { get; private set; }
    public Guid CityId { get; private set; }

    // Navigation properties
    public City City { get; private set; } = null!;
    public ICollection<Room> Rooms { get; private set; } = new List<Room>();
    public ICollection<HotelImage> Images { get; private set; } = new List<HotelImage>();
    public ICollection<Review> Reviews { get; private set; } = new List<Review>();

    // For EF Core
    private Hotel() { }

    public Hotel(string name, string description, decimal rating, Guid cityId)
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
        Rating = rating; // Initial rating, will be updated by reviews over time
        CityId = cityId;
    }

    public void Update(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Hotel name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.", nameof(description));

        Name = name;
        Description = description;
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates the hotel's average rating based on its reviews.
    /// </summary>
    /// <param name="newAverageRating">The newly calculated average rating.</param>
    public void UpdateRating(decimal newAverageRating)
    {
        if (newAverageRating < 0 || newAverageRating > 5)
        {
            throw new ArgumentException("New average rating must be between 0 and 5.", nameof(newAverageRating));
        }

        Rating = newAverageRating;
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