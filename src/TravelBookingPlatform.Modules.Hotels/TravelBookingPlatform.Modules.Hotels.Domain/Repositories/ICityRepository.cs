using TravelBookingPlatform.Core.Domain.Repositories;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

public interface ICityRepository : IRepository<City>
{
    Task<City?> GetByNameAsync(string name);
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null);
    Task<bool> PostCodeExistsAsync(string postCode, Guid? excludeId = null);

    // Search methods
    Task<IReadOnlyList<City>> GetCitySuggestionsAsync(string searchText, int maxResults = 10);
    Task<IReadOnlyList<City>> GetPopularDestinationsAsync(int count = 5);
    Task<IReadOnlyList<City>> SearchCitiesAsync(string searchText);
}