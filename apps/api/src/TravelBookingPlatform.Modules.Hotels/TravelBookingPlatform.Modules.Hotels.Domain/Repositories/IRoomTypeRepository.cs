using TravelBookingPlatform.Core.Domain.Repositories;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

public interface IRoomTypeRepository : IRepository<RoomType>
{
    Task<RoomType?> GetByNameAsync(string name);
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null);
    Task<IReadOnlyList<RoomType>> GetByCapacityAsync(int adults, int children);
    Task<IReadOnlyList<RoomType>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice);
    Task<IReadOnlyList<RoomType>> GetWithCapacityAndPriceRangeAsync(
        int adults,
        int children,
        decimal? minPrice = null,
        decimal? maxPrice = null);
}