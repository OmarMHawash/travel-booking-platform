using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TravelBookingPlatform.Core.Domain;
using TravelBookingPlatform.Modules.Identity.Application.DTOs;
using TravelBookingPlatform.Modules.Identity.Application.Services;
using TravelBookingPlatform.Modules.Identity.Domain.Entities;
using TravelBookingPlatform.Modules.Identity.Domain.Enums;
using TravelBookingPlatform.Modules.Identity.Domain.Repositories;

namespace TravelBookingPlatform.Modules.Identity.Infrastructure.Services;

public class ActivityTrackingService : IActivityTrackingService
{
    private readonly IUserActivityRepository _userActivityRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ActivityTrackingService> _logger;

    public ActivityTrackingService(
        IUserActivityRepository userActivityRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ActivityTrackingService> logger)
    {
        _userActivityRepository = userActivityRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task TrackActivityAsync(Guid userId, ActivityType activityType, Guid? targetId = null, string? targetType = null, string? metadata = null)
    {
        try
        {
            if (userId == Guid.Empty)
            {
                _logger.LogWarning("Attempted to track activity for empty user ID");
                return;
            }

            var activity = new UserActivity(userId, activityType, targetId, targetType, metadata);
            await _userActivityRepository.AddAsync(activity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogDebug("Successfully tracked activity {ActivityType} for user {UserId}", activityType, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking activity {ActivityType} for user {UserId}", activityType, userId);
            // Don't rethrow - activity tracking should not break the main flow
        }
    }

    public async Task TrackHotelViewAsync(Guid userId, Guid hotelId, string? additionalInfo = null)
    {
        var metadata = string.IsNullOrWhiteSpace(additionalInfo)
            ? null
            : JsonSerializer.Serialize(new { AdditionalInfo = additionalInfo });

        await TrackActivityAsync(userId, ActivityType.HotelView, hotelId, "Hotel", metadata);
    }

    public async Task TrackSearchActivityAsync(Guid userId, ActivityType searchType, string searchCriteria, int? resultCount = null)
    {
        var metadata = JsonSerializer.Serialize(new
        {
            SearchCriteria = searchCriteria,
            ResultCount = resultCount
        });

        await TrackActivityAsync(userId, searchType, null, null, metadata);
    }

    public async Task TrackDealViewAsync(Guid userId, Guid dealId, string? additionalInfo = null)
    {
        var metadata = string.IsNullOrWhiteSpace(additionalInfo)
            ? null
            : JsonSerializer.Serialize(new { AdditionalInfo = additionalInfo });

        await TrackActivityAsync(userId, ActivityType.DealView, dealId, "Deal", metadata);
    }

    public async Task<IReadOnlyList<UserActivityDto>> GetRecentActivitiesAsync(Guid userId, ActivityType? activityType = null, int limit = 10)
    {
        try
        {
            var activities = await _userActivityRepository.GetRecentActivitiesByUserAsync(userId, activityType, limit);
            return _mapper.Map<IReadOnlyList<UserActivityDto>>(activities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent activities for user {UserId}", userId);
            return new List<UserActivityDto>();
        }
    }

    public async Task<int> CleanupOldActivitiesAsync(DateTime beforeDate)
    {
        try
        {
            var deletedCount = await _userActivityRepository.DeleteOldActivitiesAsync(beforeDate);
            _logger.LogInformation("Cleaned up {DeletedCount} old user activities before {BeforeDate}", deletedCount, beforeDate);
            return deletedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up old activities before {BeforeDate}", beforeDate);
            return 0;
        }
    }
}