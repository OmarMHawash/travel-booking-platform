using TravelBookingPlatform.Core.Domain.Repositories;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

public interface IDealRepository : IRepository<Deal>
{
    Task<List<Deal>> GetFeaturedDealsAsync(int count = 10);
    Task<List<Deal>> GetActiveDealsAsync();
    Task<List<Deal>> GetDealsByHotelAsync(Guid hotelId);
    Task<Deal?> GetDealWithDetailsAsync(Guid dealId);
    Task<bool> HasActiveDealForRoomTypeAsync(Guid roomTypeId);
}