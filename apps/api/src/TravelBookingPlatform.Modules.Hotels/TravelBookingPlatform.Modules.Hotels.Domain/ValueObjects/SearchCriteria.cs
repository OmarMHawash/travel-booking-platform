namespace TravelBookingPlatform.Modules.Hotels.Domain.ValueObjects;

public record SearchCriteria(
    string? SearchText,
    DateTime? CheckInDate,
    DateTime? CheckOutDate,
    int NumberOfRooms,
    int Adults,
    int Children,
    decimal? MinRating,
    decimal? MaxRating,
    Guid? CityId,
    int PageNumber,
    int PageSize,
    string SortBy = "Relevance")
{
    public static SearchCriteria Create(
        string? searchText = null,
        DateTime? checkInDate = null,
        DateTime? checkOutDate = null,
        int numberOfRooms = 1,
        int adults = 2,
        int children = 0,
        decimal? minRating = null,
        decimal? maxRating = null,
        Guid? cityId = null,
        int pageNumber = 1,
        int pageSize = 20,
        string sortBy = "Relevance")
    {
        if (pageNumber < 1)
            throw new ArgumentException("Page number must be greater than 0.", nameof(pageNumber));
        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentException("Page size must be between 1 and 100.", nameof(pageSize));
        if (numberOfRooms < 1)
            throw new ArgumentException("Number of rooms must be at least 1.", nameof(numberOfRooms));
        if (adults < 1)
            throw new ArgumentException("Number of adults must be at least 1.", nameof(adults));
        if (children < 0)
            throw new ArgumentException("Number of children cannot be negative.", nameof(children));
        if (checkInDate.HasValue && checkOutDate.HasValue && checkInDate >= checkOutDate)
            throw new ArgumentException("Check-in date must be before check-out date.");
        if (checkInDate.HasValue && checkInDate.Value.Date < DateTime.Today)
            throw new ArgumentException("Check-in date cannot be in the past.");
        if (minRating.HasValue && (minRating < 0 || minRating > 5))
            throw new ArgumentException("Minimum rating must be between 0 and 5.", nameof(minRating));
        if (maxRating.HasValue && (maxRating < 0 || maxRating > 5))
            throw new ArgumentException("Maximum rating must be between 0 and 5.", nameof(maxRating));
        if (minRating.HasValue && maxRating.HasValue && minRating > maxRating)
            throw new ArgumentException("Minimum rating cannot be greater than maximum rating.");

        return new SearchCriteria(
            searchText?.Trim(),
            checkInDate?.Date,
            checkOutDate?.Date,
            numberOfRooms,
            adults,
            children,
            minRating,
            maxRating,
            cityId,
            pageNumber,
            pageSize,
            sortBy);
    }

    public bool HasDateRange => CheckInDate.HasValue && CheckOutDate.HasValue;
    public bool HasRatingFilter => MinRating.HasValue || MaxRating.HasValue;
    public bool HasSearch => !string.IsNullOrWhiteSpace(SearchText);
    public bool HasGuestRequirements => Adults > 0 || Children > 0;
}