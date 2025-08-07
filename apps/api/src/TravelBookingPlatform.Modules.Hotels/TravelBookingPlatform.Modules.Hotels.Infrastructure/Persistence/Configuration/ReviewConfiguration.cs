using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Infrastructure.Persistence.Configuration;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Review");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.StarRating).IsRequired();
        builder.Property(r => r.Description).IsRequired().HasMaxLength(4000);

        // Foreign Keys
        builder.Property(r => r.HotelId).IsRequired();
        builder.Property(r => r.BookingId).IsRequired();
        builder.Property(r => r.UserId).IsRequired();

        builder.HasIndex(r => r.HotelId).HasDatabaseName("IX_Review_HotelId");
        builder.HasIndex(r => r.UserId).HasDatabaseName("IX_Review_UserId");

        builder.HasIndex(r => r.BookingId)
            .IsUnique()
            .HasDatabaseName("IX_Review_BookingId_Unique");

        // Relationships
        builder.HasOne(r => r.Hotel)
            .WithMany(h => h.Reviews)
            .HasForeignKey(r => r.HotelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Booking)
            .WithOne()
            .HasForeignKey<Review>(r => r.BookingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}