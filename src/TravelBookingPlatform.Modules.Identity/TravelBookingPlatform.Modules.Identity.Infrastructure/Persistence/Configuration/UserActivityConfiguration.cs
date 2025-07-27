using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelBookingPlatform.Modules.Identity.Domain.Entities;
using TravelBookingPlatform.Modules.Identity.Domain.Enums;

namespace TravelBookingPlatform.Modules.Identity.Infrastructure.Persistence.Configuration;

public class UserActivityConfiguration : IEntityTypeConfiguration<UserActivity>
{
    public void Configure(EntityTypeBuilder<UserActivity> builder)
    {
        builder.ToTable("UserActivities");

        builder.HasKey(ua => ua.Id);

        builder.Property(ua => ua.UserId)
            .IsRequired();

        builder.Property(ua => ua.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(ua => ua.TargetId)
            .IsRequired(false);

        builder.Property(ua => ua.TargetType)
            .IsRequired(false)
            .HasMaxLength(50);

        builder.Property(ua => ua.Metadata)
            .IsRequired(false)
            .HasMaxLength(2000);

        builder.Property(ua => ua.ActivityDate)
            .IsRequired();

        builder.Property(ua => ua.CreatedAt)
            .IsRequired();

        builder.Property(ua => ua.UpdatedAt)
            .IsRequired(false);

        // Foreign key relationship to User
        builder.HasOne(ua => ua.User)
            .WithMany()
            .HasForeignKey(ua => ua.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for efficient querying
        // Most common query: recent activities by user and type
        builder.HasIndex(ua => new { ua.UserId, ua.Type, ua.ActivityDate })
            .HasDatabaseName("IX_UserActivities_User_Type_Date");

        // For recently visited queries: user, target type, activity date
        builder.HasIndex(ua => new { ua.UserId, ua.TargetType, ua.ActivityDate })
            .HasDatabaseName("IX_UserActivities_User_TargetType_Date");

        // For checking specific activity: user, target, type
        builder.HasIndex(ua => new { ua.UserId, ua.TargetId, ua.TargetType, ua.Type })
            .HasDatabaseName("IX_UserActivities_User_Target_Type");

        // For cleanup operations: activity date
        builder.HasIndex(ua => ua.ActivityDate)
            .HasDatabaseName("IX_UserActivities_ActivityDate");
    }
}