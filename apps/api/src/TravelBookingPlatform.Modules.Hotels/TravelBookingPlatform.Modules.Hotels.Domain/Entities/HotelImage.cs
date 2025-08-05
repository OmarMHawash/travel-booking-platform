using TravelBookingPlatform.Core.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Domain.Entities;

public class HotelImage : AggregateRoot
{
    public Guid HotelId { get; private set; }
    public string Url { get; private set; }
    public string? Caption { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsCoverImage { get; private set; }

    // Navigation property
    public Hotel Hotel { get; private set; } = null!;

    private HotelImage() { } // For EF Core

    public HotelImage(Guid hotelId, string url, string? caption, int sortOrder, bool isCoverImage = false)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Image URL cannot be empty.", nameof(url));
        if (hotelId == Guid.Empty)
            throw new ArgumentException("Hotel ID cannot be empty.", nameof(hotelId));

        HotelId = hotelId;
        Url = url;
        Caption = caption;
        SortOrder = sortOrder;
        IsCoverImage = isCoverImage;
    }

    public void SetAsCoverImage() => IsCoverImage = true;
    public void UnsetAsCoverImage() => IsCoverImage = false;
}