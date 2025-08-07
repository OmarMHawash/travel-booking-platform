using TravelBookingPlatform.Core.Domain.Repositories;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

public interface IBookingRepository : IRepository<Booking>
{
    Task<IReadOnlyList<Booking>> GetByUserIdAsync(Guid userId);
    Task<IReadOnlyList<Booking>> GetByRoomIdAsync(Guid roomId);

    Task<Booking?> GetByIdWithDetailsAsync(Guid bookingId);
    Task<IReadOnlyList<Booking>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IReadOnlyList<Booking>> GetOverlappingBookingsAsync(
        Guid roomId,
        DateTime checkInDate,
        DateTime checkOutDate);
    Task<bool> HasOverlappingBookingAsync(
        Guid roomId,
        DateTime checkInDate,
        DateTime checkOutDate,
        Guid? excludeBookingId = null);
    Task<IReadOnlyList<Booking>> GetUserBookingsInDateRangeAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate);
    Task<IReadOnlyList<Booking>> GetUpcomingBookingsAsync(Guid userId);
    Task<IReadOnlyList<Booking>> GetPastBookingsAsync(Guid userId);
    Task<bool> UserHasBookingInPeriodAsync(
        Guid userId,
        DateTime checkInDate,
        DateTime checkOutDate);
}