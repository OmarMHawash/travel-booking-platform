using TravelBookingPlatform.Core.Domain.Repositories;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;
using TravelBookingPlatform.Modules.Hotels.Domain.ValueObjects;

namespace TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

public interface IHotelRepository : IRepository<Hotel>
{
    Task<IReadOnlyList<Hotel>> GetByCityIdAsync(Guid cityId);
    Task<IReadOnlyList<Hotel>> SearchByNameAsync(string searchTerm);
    Task<IReadOnlyList<Hotel>> SearchByCityNameAsync(string cityName);
    Task<IReadOnlyList<Hotel>> GetByRatingRangeAsync(decimal minRating, decimal maxRating);
    Task<Hotel?> GetByNameAsync(string name);
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null);
    Task<IReadOnlyList<Hotel>> GetHotelsWithAvailableRoomsAsync(
        DateTime checkInDate,
        DateTime checkOutDate,
        int adults,
        int children);
    Task<IReadOnlyList<Hotel>> SearchWithFiltersAsync(
        string? searchTerm = null,
        Guid? cityId = null,
        decimal? minRating = null,
        decimal? maxRating = null,
        DateTime? checkInDate = null,
        DateTime? checkOutDate = null,
        int? adults = null,
        int? children = null);

    // New enhanced search methods
    Task<IReadOnlyList<Hotel>> GetHotelSuggestionsAsync(string searchText, int maxResults = 10);
    Task<(IReadOnlyList<Hotel> Hotels, int TotalCount)> SearchHotelsWithPaginationAsync(SearchCriteria criteria);
}