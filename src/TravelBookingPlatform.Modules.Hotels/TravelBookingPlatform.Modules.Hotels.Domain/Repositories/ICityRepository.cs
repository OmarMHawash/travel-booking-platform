using TravelBookingPlatform.Core.Domain.Repositories;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

public interface ICityRepository : IRepository<City>
{
    // Add specific methods for City if needed beyond generic CRUD
    Task<City?> GetByNameAsync(string name);
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null);
    Task<bool> PostCodeExistsAsync(string postCode, Guid? excludeId = null);
}