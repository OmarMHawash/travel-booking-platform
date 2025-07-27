using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Infrastructure.Persistence.Configuration;

public class RoomTypeConfiguration : IEntityTypeConfiguration<RoomType>
{
    public void Configure(EntityTypeBuilder<RoomType> builder)
    {
        builder.ToTable("RoomType");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(rt => rt.PricePerNight)
            .IsRequired()
            .HasColumnType("decimal(10,2)"); // e.g., 9999999.99

        builder.Property(rt => rt.MaxAdults)
            .IsRequired();

        builder.Property(rt => rt.MaxChildren)
            .IsRequired();

        builder.Property(rt => rt.CreatedAt)
            .IsRequired();

        builder.Property(rt => rt.UpdatedAt)
            .IsRequired(false);

        // Indexes for performance
        builder.HasIndex(rt => rt.Name)
            .HasDatabaseName("IX_RoomType_Name");

        builder.HasIndex(rt => rt.PricePerNight)
            .HasDatabaseName("IX_RoomType_PricePerNight");

        builder.HasIndex(rt => new { rt.MaxAdults, rt.MaxChildren })
            .HasDatabaseName("IX_RoomType_Capacity");

        // Unique constraint for room type name
        builder.HasIndex(rt => rt.Name)
            .IsUnique()
            .HasDatabaseName("IX_RoomType_Name_Unique");

        // Relationships
        builder.HasMany(rt => rt.Rooms)
            .WithOne(r => r.RoomType)
            .HasForeignKey(r => r.RoomTypeId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent deleting room type if rooms exist
    }
}