using TravelBookingPlatform.Core.Domain.Repositories;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

public interface IRoomRepository : IRepository<Room>
{
    Task<IReadOnlyList<Room>> GetByHotelIdAsync(Guid hotelId);
    Task<IReadOnlyList<Room>> GetByRoomTypeIdAsync(Guid roomTypeId);
    Task<Room?> GetByHotelAndRoomNumberAsync(Guid hotelId, string roomNumber);
    Task<bool> RoomNumberExistsInHotelAsync(Guid hotelId, string roomNumber, Guid? excludeRoomId = null);
    Task<IReadOnlyList<Room>> GetAvailableRoomsAsync(
        DateTime checkInDate,
        DateTime checkOutDate);
    Task<IReadOnlyList<Room>> GetAvailableRoomsForHotelAsync(
        Guid hotelId,
        DateTime checkInDate,
        DateTime checkOutDate);
    Task<IReadOnlyList<Room>> GetAvailableRoomsByTypeAsync(
        Guid roomTypeId,
        DateTime checkInDate,
        DateTime checkOutDate);
    Task<IReadOnlyList<Room>> GetAvailableRoomsWithCapacityAsync(
        DateTime checkInDate,
        DateTime checkOutDate,
        int adults,
        int children);

    Task<IReadOnlyList<Room>> GetAvailableRoomsByTypeForPeriodAsync(
        Guid hotelId,
        Guid roomTypeId,
        DateTime checkInDate,
        DateTime checkOutDate);
}