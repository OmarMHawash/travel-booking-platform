using TravelBookingPlatform.Modules.Identity.Domain.Enums;

namespace TravelBookingPlatform.Modules.Identity.Application.DTOs;

public class UserActivityDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public ActivityType Type { get; set; }
    public Guid? TargetId { get; set; }
    public string? TargetType { get; set; }
    public string? Metadata { get; set; }
    public DateTime ActivityDate { get; set; }
}