using TravelBookingPlatform.Core.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Domain.Entities;

public class Deal : AggregateRoot
{
    public string Title { get; private set; }
    public string Description { get; private set; }
    public Guid HotelId { get; private set; }
    public Guid? RoomTypeId { get; private set; }
    public decimal OriginalPrice { get; private set; }
    public decimal DiscountedPrice { get; private set; }
    public decimal DiscountPercentage { get; private set; }
    public DateTime ValidFrom { get; private set; }
    public DateTime ValidTo { get; private set; }
    public bool IsFeatured { get; private set; }
    public bool IsActive { get; private set; }
    public int MaxBookings { get; private set; }
    public int CurrentBookings { get; private set; }
    public string? ImageURL { get; private set; }

    // Navigation properties
    public Hotel Hotel { get; private set; } = null!;
    public RoomType? RoomType { get; private set; }

    // For EF Core
    private Deal() { }

    public Deal(string title, string description, Guid hotelId, decimal originalPrice, decimal discountedPrice,
               DateTime validFrom, DateTime validTo, bool isFeatured = false, Guid roomTypeId = default,
               int maxBookings = 100, string? imageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Deal title cannot be empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Deal description cannot be empty.", nameof(description));
        if (hotelId == Guid.Empty)
            throw new ArgumentException("Hotel ID cannot be empty.", nameof(hotelId));
        if (roomTypeId == Guid.Empty)
            throw new ArgumentException("Room type ID cannot be empty.", nameof(roomTypeId));
        if (originalPrice <= 0)
            throw new ArgumentException("Original price must be greater than zero.", nameof(originalPrice));
        if (discountedPrice <= 0)
            throw new ArgumentException("Discounted price must be greater than zero.", nameof(discountedPrice));
        if (discountedPrice >= originalPrice)
            throw new ArgumentException("Discounted price must be less than original price.", nameof(discountedPrice));
        if (validFrom >= validTo)
            throw new ArgumentException("Valid from date must be before valid to date.", nameof(validFrom));
        if (maxBookings <= 0)
            throw new ArgumentException("Max bookings must be greater than zero.", nameof(maxBookings));

        Title = title;
        Description = description;
        HotelId = hotelId;
        RoomTypeId = roomTypeId;
        OriginalPrice = originalPrice;
        DiscountedPrice = discountedPrice;
        DiscountPercentage = Math.Round(((originalPrice - discountedPrice) / originalPrice) * 100, 2);
        ValidFrom = validFrom;
        ValidTo = validTo;
        IsFeatured = isFeatured;
        IsActive = true;
        MaxBookings = maxBookings;
        CurrentBookings = 0;
        ImageURL = imageUrl;
    }

    public void Update(string title, string description, Guid roomTypeId, decimal originalPrice, decimal discountedPrice,
                      DateTime validFrom, DateTime validTo, int maxBookings, string? imageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Deal title cannot be empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Deal description cannot be empty.", nameof(description));
        if (roomTypeId == Guid.Empty)
            throw new ArgumentException("Room type ID cannot be empty.", nameof(roomTypeId));
        if (originalPrice <= 0)
            throw new ArgumentException("Original price must be greater than zero.", nameof(originalPrice));
        if (discountedPrice <= 0)
            throw new ArgumentException("Discounted price must be greater than zero.", nameof(discountedPrice));
        if (discountedPrice >= originalPrice)
            throw new ArgumentException("Discounted price must be less than original price.", nameof(discountedPrice));
        if (validFrom >= validTo)
            throw new ArgumentException("Valid from date must be before valid to date.", nameof(validFrom));
        if (maxBookings <= 0)
            throw new ArgumentException("Max bookings must be greater than zero.", nameof(maxBookings));

        Title = title;
        Description = description;
        RoomTypeId = roomTypeId;
        OriginalPrice = originalPrice;
        DiscountedPrice = discountedPrice;
        DiscountPercentage = Math.Round(((originalPrice - discountedPrice) / originalPrice) * 100, 2);
        ValidFrom = validFrom;
        ValidTo = validTo;
        MaxBookings = maxBookings;
        ImageURL = imageUrl;
        MarkAsUpdated();
    }

    public void ToggleFeatured()
    {
        IsFeatured = !IsFeatured;
        MarkAsUpdated();
    }

    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }

    public bool IsValid() => IsActive && DateTime.UtcNow >= ValidFrom && DateTime.UtcNow <= ValidTo;

    public bool IsAvailable() => IsValid() && CurrentBookings < MaxBookings;

    public void IncrementBookings()
    {
        if (!IsAvailable())
            throw new InvalidOperationException("Deal is not available for booking.");

        CurrentBookings++;
        MarkAsUpdated();
    }
}