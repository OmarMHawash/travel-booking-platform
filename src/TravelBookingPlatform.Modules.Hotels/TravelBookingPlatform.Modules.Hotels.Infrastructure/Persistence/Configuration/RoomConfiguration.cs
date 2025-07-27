using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Infrastructure.Persistence.Configuration;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("Room");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.RoomNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.HotelId)
            .IsRequired();

        builder.Property(r => r.RoomTypeId)
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .IsRequired(false);

        // Indexes for performance
        builder.HasIndex(r => r.HotelId)
            .HasDatabaseName("IX_Room_HotelId");

        builder.HasIndex(r => r.RoomTypeId)
            .HasDatabaseName("IX_Room_RoomTypeId");

        builder.HasIndex(r => r.RoomNumber)
            .HasDatabaseName("IX_Room_RoomNumber");

        // Unique constraint: Room number must be unique within each hotel
        builder.HasIndex(r => new { r.HotelId, r.RoomNumber })
            .IsUnique()
            .HasDatabaseName("IX_Room_HotelId_RoomNumber_Unique");

        // Relationships
        builder.HasOne(r => r.Hotel)
            .WithMany(h => h.Rooms)
            .HasForeignKey(r => r.HotelId)
            .OnDelete(DeleteBehavior.Cascade); // Delete room when hotel is deleted

        builder.HasOne(r => r.RoomType)
            .WithMany(rt => rt.Rooms)
            .HasForeignKey(r => r.RoomTypeId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent deleting room type if rooms exist

        builder.HasMany(r => r.Bookings)
            .WithOne(b => b.Room)
            .HasForeignKey(b => b.RoomId)
            .OnDelete(DeleteBehavior.Cascade); // Delete bookings when room is deleted
    }
}