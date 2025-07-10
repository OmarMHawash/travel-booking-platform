using TravelBookingPlatform.Modules.Identity.Application.DTOs;
using TravelBookingPlatform.Modules.Identity.Domain.Enums;

namespace TravelBookingPlatform.Modules.Identity.Application.Services;

public interface IActivityTrackingService
{
    /// <summary>
    /// Tracks user activity asynchronously without blocking the main request flow
    /// </summary>
    Task TrackActivityAsync(Guid userId, ActivityType activityType, Guid? targetId = null, string? targetType = null, string? metadata = null);

    /// <summary>
    /// Tracks hotel view activity with specific hotel information
    /// </summary>
    Task TrackHotelViewAsync(Guid userId, Guid hotelId, string? additionalInfo = null);

    /// <summary>
    /// Tracks search activity with search criteria
    /// </summary>
    Task TrackSearchActivityAsync(Guid userId, ActivityType searchType, string searchCriteria, int? resultCount = null);

    /// <summary>
    /// Tracks deal view activity
    /// </summary>
    Task TrackDealViewAsync(Guid userId, Guid dealId, string? additionalInfo = null);

    /// <summary>
    /// Gets recent activities for a user (for recently visited functionality)
    /// </summary>
    Task<IReadOnlyList<UserActivityDto>> GetRecentActivitiesAsync(Guid userId, ActivityType? activityType = null, int limit = 10);

    /// <summary>
    /// Cleans up old user activities based on retention policy
    /// </summary>
    Task<int> CleanupOldActivitiesAsync(DateTime beforeDate);
}