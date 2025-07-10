using Microsoft.EntityFrameworkCore;
using TravelBookingPlatform.Modules.Identity.Domain.Entities;
using TravelBookingPlatform.Modules.Identity.Domain.Enums;
using TravelBookingPlatform.Modules.Identity.Domain.Repositories;
using TravelBookingPlatform.SharedInfrastructure.Persistence;

namespace TravelBookingPlatform.Modules.Identity.Infrastructure.Persistence.Repositories;

public class UserActivityRepository : BaseRepository<UserActivity>, IUserActivityRepository
{
    public UserActivityRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<UserActivity>> GetRecentActivitiesByUserAsync(Guid userId, ActivityType? activityType = null, int limit = 10)
    {
        var query = _dbSet.Where(a => a.UserId == userId);

        if (activityType.HasValue)
        {
            query = query.Where(a => a.Type == activityType.Value);
        }

        return await query
            .OrderByDescending(a => a.ActivityDate)
            .Take(limit)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IReadOnlyList<UserActivity>> GetActivitiesByUserAndTargetAsync(Guid userId, Guid targetId, string targetType)
    {
        return await _dbSet
            .Where(a => a.UserId == userId && a.TargetId == targetId && a.TargetType == targetType)
            .OrderByDescending(a => a.ActivityDate)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<UserActivity?> GetLatestActivityAsync(Guid userId, Guid targetId, string targetType, ActivityType activityType)
    {
        return await _dbSet
            .Where(a => a.UserId == userId && a.TargetId == targetId && a.TargetType == targetType && a.Type == activityType)
            .OrderByDescending(a => a.ActivityDate)
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<UserActivity>> GetActivitiesAsync(Guid userId, ActivityType? activityType = null, DateTime? since = null, int page = 1, int pageSize = 20)
    {
        var query = _dbSet.Where(a => a.UserId == userId);

        if (activityType.HasValue)
        {
            query = query.Where(a => a.Type == activityType.Value);
        }

        if (since.HasValue)
        {
            query = query.Where(a => a.ActivityDate >= since.Value);
        }

        return await query
            .OrderByDescending(a => a.ActivityDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> DeleteOldActivitiesAsync(DateTime beforeDate)
    {
        var activitiesToDelete = await _dbSet
            .Where(a => a.ActivityDate < beforeDate)
            .ToListAsync();

        if (activitiesToDelete.Count > 0)
        {
            _dbSet.RemoveRange(activitiesToDelete);
            return await _dbContext.SaveChangesAsync();
        }

        return 0;
    }

    public async Task<int> GetActivityCountAsync(Guid userId, ActivityType? activityType = null, DateTime? since = null)
    {
        var query = _dbSet.Where(a => a.UserId == userId);

        if (activityType.HasValue)
        {
            query = query.Where(a => a.Type == activityType.Value);
        }

        if (since.HasValue)
        {
            query = query.Where(a => a.ActivityDate >= since.Value);
        }

        return await query.CountAsync();
    }
}