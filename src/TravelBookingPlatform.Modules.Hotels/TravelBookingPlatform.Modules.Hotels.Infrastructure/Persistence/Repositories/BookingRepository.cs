using Microsoft.EntityFrameworkCore;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;
using TravelBookingPlatform.SharedInfrastructure.Persistence;

namespace TravelBookingPlatform.Modules.Hotels.Infrastructure.Persistence.Repositories;

public class BookingRepository : BaseRepository<Booking>, IBookingRepository
{
    public BookingRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<Booking>> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Where(b => b.UserId == userId)
            .Include(b => b.Room)
                .ThenInclude(r => r.Hotel)
                    .ThenInclude(h => h.City)
            .Include(b => b.Room)
                .ThenInclude(r => r.RoomType)
            .OrderByDescending(b => b.CheckInDate)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Booking>> GetByRoomIdAsync(Guid roomId)
    {
        return await _dbSet
            .Where(b => b.RoomId == roomId)
            .Include(b => b.Room)
                .ThenInclude(r => r.Hotel)
                    .ThenInclude(h => h.City)
            .Include(b => b.Room)
                .ThenInclude(r => r.RoomType)
            .OrderBy(b => b.CheckInDate)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Booking>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(b => b.CheckInDate <= endDate && b.CheckOutDate >= startDate)
            .Include(b => b.Room)
                .ThenInclude(r => r.Hotel)
                    .ThenInclude(h => h.City)
            .Include(b => b.Room)
                .ThenInclude(r => r.RoomType)
            .OrderBy(b => b.CheckInDate)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Booking>> GetOverlappingBookingsAsync(
        Guid roomId,
        DateTime checkInDate,
        DateTime checkOutDate)
    {
        return await _dbSet
            .Where(b => b.RoomId == roomId &&
                b.CheckInDate < checkOutDate && b.CheckOutDate > checkInDate)
            .Include(b => b.Room)
                .ThenInclude(r => r.Hotel)
                    .ThenInclude(h => h.City)
            .Include(b => b.Room)
                .ThenInclude(r => r.RoomType)
            .OrderBy(b => b.CheckInDate)
            .ToListAsync();
    }

    public async Task<bool> HasOverlappingBookingAsync(
        Guid roomId,
        DateTime checkInDate,
        DateTime checkOutDate,
        Guid? excludeBookingId = null)
    {
        var query = _dbSet
            .Where(b => b.RoomId == roomId &&
                b.CheckInDate < checkOutDate && b.CheckOutDate > checkInDate);

        if (excludeBookingId.HasValue)
        {
            query = query.Where(b => b.Id != excludeBookingId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<IReadOnlyList<Booking>> GetUserBookingsInDateRangeAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate)
    {
        return await _dbSet
            .Where(b => b.UserId == userId &&
                b.CheckInDate <= endDate && b.CheckOutDate >= startDate)
            .Include(b => b.Room)
                .ThenInclude(r => r.Hotel)
                    .ThenInclude(h => h.City)
            .Include(b => b.Room)
                .ThenInclude(r => r.RoomType)
            .OrderBy(b => b.CheckInDate)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Booking>> GetUpcomingBookingsAsync(Guid userId)
    {
        var today = DateTime.Today;
        return await _dbSet
            .Where(b => b.UserId == userId && b.CheckInDate >= today)
            .Include(b => b.Room)
                .ThenInclude(r => r.Hotel)
                    .ThenInclude(h => h.City)
            .Include(b => b.Room)
                .ThenInclude(r => r.RoomType)
            .OrderBy(b => b.CheckInDate)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Booking>> GetPastBookingsAsync(Guid userId)
    {
        var today = DateTime.Today;
        return await _dbSet
            .Where(b => b.UserId == userId && b.CheckOutDate < today)
            .Include(b => b.Room)
                .ThenInclude(r => r.Hotel)
                    .ThenInclude(h => h.City)
            .Include(b => b.Room)
                .ThenInclude(r => r.RoomType)
            .OrderByDescending(b => b.CheckOutDate)
            .ToListAsync();
    }

    public async Task<bool> UserHasBookingInPeriodAsync(
        Guid userId,
        DateTime checkInDate,
        DateTime checkOutDate)
    {
        return await _dbSet
            .AnyAsync(b => b.UserId == userId &&
                b.CheckInDate < checkOutDate && b.CheckOutDate > checkInDate);
    }
}