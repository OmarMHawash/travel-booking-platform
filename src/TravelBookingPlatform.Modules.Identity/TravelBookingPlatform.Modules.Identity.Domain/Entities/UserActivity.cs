using TravelBookingPlatform.Core.Domain.Entities;
using TravelBookingPlatform.Modules.Identity.Domain.Enums;

namespace TravelBookingPlatform.Modules.Identity.Domain.Entities;

public class UserActivity : AggregateRoot
{
    public Guid UserId { get; private set; }
    public ActivityType Type { get; private set; }
    public Guid? TargetId { get; private set; }
    public string? TargetType { get; private set; }
    public string? Metadata { get; private set; }
    public DateTime ActivityDate { get; private set; }

    // Navigation property
    public User User { get; private set; } = null!;

    // For EF Core
    private UserActivity() { }

    public UserActivity(Guid userId, ActivityType type, Guid? targetId = null, string? targetType = null, string? metadata = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        if (targetId.HasValue && string.IsNullOrWhiteSpace(targetType))
            throw new ArgumentException("Target type is required when target ID is provided.", nameof(targetType));

        if (!string.IsNullOrWhiteSpace(targetType) && targetType.Length > 50)
            throw new ArgumentException("Target type cannot exceed 50 characters.", nameof(targetType));

        if (!string.IsNullOrWhiteSpace(metadata) && metadata.Length > 2000)
            throw new ArgumentException("Metadata cannot exceed 2000 characters.", nameof(metadata));

        UserId = userId;
        Type = type;
        TargetId = targetId;
        TargetType = targetType;
        Metadata = metadata;
        ActivityDate = DateTime.UtcNow;
    }

    public void UpdateMetadata(string? newMetadata)
    {
        if (!string.IsNullOrWhiteSpace(newMetadata) && newMetadata.Length > 2000)
            throw new ArgumentException("Metadata cannot exceed 2000 characters.", nameof(newMetadata));

        Metadata = newMetadata;
        MarkAsUpdated();
    }

    public bool IsRecentActivity(TimeSpan maxAge)
    {
        return DateTime.UtcNow - ActivityDate <= maxAge;
    }

    public bool IsActivityType(ActivityType activityType)
    {
        return Type == activityType;
    }

    public bool IsTargetType(string targetType)
    {
        return string.Equals(TargetType, targetType, StringComparison.OrdinalIgnoreCase);
    }
}