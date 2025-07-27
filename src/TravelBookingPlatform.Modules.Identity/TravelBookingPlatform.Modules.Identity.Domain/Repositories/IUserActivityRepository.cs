using TravelBookingPlatform.Core.Domain.Repositories;
using TravelBookingPlatform.Modules.Identity.Domain.Entities;
using TravelBookingPlatform.Modules.Identity.Domain.Enums;

namespace TravelBookingPlatform.Modules.Identity.Domain.Repositories;

public interface IUserActivityRepository : IRepository<UserActivity>
{
    Task<IReadOnlyList<UserActivity>> GetRecentActivitiesByUserAsync(Guid userId, ActivityType? activityType = null, int limit = 10);
    Task<IReadOnlyList<UserActivity>> GetActivitiesByUserAndTargetAsync(Guid userId, Guid targetId, string targetType);
    Task<UserActivity?> GetLatestActivityAsync(Guid userId, Guid targetId, string targetType, ActivityType activityType);
    Task<IReadOnlyList<UserActivity>> GetActivitiesAsync(Guid userId, ActivityType? activityType = null, DateTime? since = null, int page = 1, int pageSize = 20);
    Task<int> DeleteOldActivitiesAsync(DateTime beforeDate);
    Task<int> GetActivityCountAsync(Guid userId, ActivityType? activityType = null, DateTime? since = null);
}