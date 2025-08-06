using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Infrastructure.Persistence.Configuration;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Booking");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.CheckInDate)
            .IsRequired()
            .HasColumnType("date"); // Store only date part

        builder.Property(b => b.CheckOutDate)
            .IsRequired()
            .HasColumnType("date"); // Store only date part

        builder.Property(b => b.RoomId)
            .IsRequired();

        builder.Property(b => b.UserId)
            .IsRequired(); // Loose coupling - no navigation property to User

        builder.Property(b => b.CreatedAt)
            .IsRequired();

        builder.Property(b => b.UpdatedAt)
            .IsRequired(false);

        // Indexes for performance
        builder.HasIndex(b => b.RoomId)
            .HasDatabaseName("IX_Booking_RoomId");

        builder.HasIndex(b => b.UserId)
            .HasDatabaseName("IX_Booking_UserId");

        builder.HasIndex(b => b.CheckInDate)
            .HasDatabaseName("IX_Booking_CheckInDate");

        builder.HasIndex(b => b.CheckOutDate)
            .HasDatabaseName("IX_Booking_CheckOutDate");

        // Composite index for availability queries (most important for performance)
        builder.HasIndex(b => new { b.RoomId, b.CheckInDate, b.CheckOutDate })
            .HasDatabaseName("IX_Booking_Room_Dates");

        // Index for user bookings queries
        builder.HasIndex(b => new { b.UserId, b.CheckInDate })
            .HasDatabaseName("IX_Booking_User_CheckIn");

        // Relationships
        builder.HasOne(b => b.Room)
            .WithMany(r => r.Bookings)
            .HasForeignKey(b => b.RoomId)
            .OnDelete(DeleteBehavior.Cascade); // Delete booking when room is deleted

        // Note: No foreign key constraint to User table (loose coupling)
        // UserId is stored as Guid but not enforced by database

        builder.Property(b => b.HasBeenReviewed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(b => new { b.UserId, b.CheckOutDate, b.HasBeenReviewed })
            .HasDatabaseName("IX_Booking_Reviewable");
    }
}