using TravelBookingPlatform.Core.Domain.Repositories;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

public interface IReviewRepository : IRepository<Review>
{
    /// <summary>
    /// Gets a review by its associated booking ID.
    /// </summary>
    Task<Review?> GetByBookingIdAsync(Guid bookingId);

    /// <summary>
    /// Gets a paginated list of reviews for a specific hotel.
    /// </summary>
    Task<IReadOnlyList<Review>> GetByHotelIdAsync(Guid hotelId, int pageNumber, int pageSize);

    /// <summary>
    /// Gets the total count of reviews for a specific hotel.
    /// </summary>
    Task<int> GetCountByHotelIdAsync(Guid hotelId);

    /// <summary>
    /// Retrieves all star ratings for a given hotel to calculate the average.
    /// </summary>
    Task<IReadOnlyList<int>> GetAllRatingsForHotelAsync(Guid hotelId);
}