using Microsoft.EntityFrameworkCore;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;
using TravelBookingPlatform.SharedInfrastructure.Persistence;

namespace TravelBookingPlatform.Modules.Hotels.Infrastructure.Persistence.Repositories;

public class RoomRepository : BaseRepository<Room>, IRoomRepository
{
    public RoomRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<Room>> GetByHotelIdAsync(Guid hotelId)
    {
        return await _dbSet
            .Where(r => r.HotelId == hotelId)
            .Include(r => r.RoomType)
            .Include(r => r.Hotel)
            .OrderBy(r => r.RoomNumber)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Room>> GetByRoomTypeIdAsync(Guid roomTypeId)
    {
        return await _dbSet
            .Where(r => r.RoomTypeId == roomTypeId)
            .Include(r => r.RoomType)
            .Include(r => r.Hotel)
                .ThenInclude(h => h.City)
            .OrderBy(r => r.Hotel.Name)
            .ThenBy(r => r.RoomNumber)
            .ToListAsync();
    }

    public async Task<Room?> GetByHotelAndRoomNumberAsync(Guid hotelId, string roomNumber)
    {
        return await _dbSet
            .Include(r => r.RoomType)
            .Include(r => r.Hotel)
                .ThenInclude(h => h.City)
            .FirstOrDefaultAsync(r => r.HotelId == hotelId && r.RoomNumber == roomNumber);
    }

    public async Task<bool> RoomNumberExistsInHotelAsync(Guid hotelId, string roomNumber, Guid? excludeRoomId = null)
    {
        var query = _dbSet
            .Where(r => r.HotelId == hotelId && r.RoomNumber == roomNumber);

        if (excludeRoomId.HasValue)
        {
            query = query.Where(r => r.Id != excludeRoomId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<IReadOnlyList<Room>> GetAvailableRoomsAsync(
        DateTime checkInDate,
        DateTime checkOutDate)
    {
        return await _dbSet
            .Where(r => !r.Bookings.Any(b =>
                b.CheckInDate < checkOutDate && b.CheckOutDate > checkInDate))
            .Include(r => r.RoomType)
            .Include(r => r.Hotel)
                .ThenInclude(h => h.City)
            .OrderBy(r => r.Hotel.Name)
            .ThenBy(r => r.RoomNumber)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Room>> GetAvailableRoomsForHotelAsync(
        Guid hotelId,
        DateTime checkInDate,
        DateTime checkOutDate)
    {
        return await _dbSet
            .Where(r => r.HotelId == hotelId &&
                !r.Bookings.Any(b =>
                    b.CheckInDate < checkOutDate && b.CheckOutDate > checkInDate))
            .Include(r => r.RoomType)
            .Include(r => r.Hotel)
            .OrderBy(r => r.RoomNumber)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Room>> GetAvailableRoomsByTypeAsync(
        Guid roomTypeId,
        DateTime checkInDate,
        DateTime checkOutDate)
    {
        return await _dbSet
            .Where(r => r.RoomTypeId == roomTypeId &&
                !r.Bookings.Any(b =>
                    b.CheckInDate < checkOutDate && b.CheckOutDate > checkInDate))
            .Include(r => r.RoomType)
            .Include(r => r.Hotel)
                .ThenInclude(h => h.City)
            .OrderBy(r => r.Hotel.Name)
            .ThenBy(r => r.RoomNumber)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Room>> GetAvailableRoomsWithCapacityAsync(
        DateTime checkInDate,
        DateTime checkOutDate,
        int adults,
        int children)
    {
        return await _dbSet
            .Where(r => r.RoomType.MaxAdults >= adults &&
                r.RoomType.MaxChildren >= children &&
                !r.Bookings.Any(b =>
                    b.CheckInDate < checkOutDate && b.CheckOutDate > checkInDate))
            .Include(r => r.RoomType)
            .Include(r => r.Hotel)
                .ThenInclude(h => h.City)
            .OrderBy(r => r.RoomType.PricePerNight)
            .ThenBy(r => r.Hotel.Name)
            .ThenBy(r => r.RoomNumber)
            .ToListAsync();
    }
}