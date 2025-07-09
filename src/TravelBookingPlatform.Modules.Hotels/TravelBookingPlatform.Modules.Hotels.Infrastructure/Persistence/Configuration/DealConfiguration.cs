using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Infrastructure.Persistence.Configuration;

public class DealConfiguration : IEntityTypeConfiguration<Deal>
{
    public void Configure(EntityTypeBuilder<Deal> builder)
    {
        builder.ToTable("Deal");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(d => d.OriginalPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(d => d.DiscountedPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(d => d.DiscountPercentage)
            .IsRequired()
            .HasPrecision(5, 2);

        builder.Property(d => d.ValidFrom)
            .IsRequired();

        builder.Property(d => d.ValidTo)
            .IsRequired();

        builder.Property(d => d.IsFeatured)
            .IsRequired();

        builder.Property(d => d.IsActive)
            .IsRequired();

        builder.Property(d => d.MaxBookings)
            .IsRequired();

        builder.Property(d => d.CurrentBookings)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(d => d.ImageURL)
            .HasMaxLength(500);

        builder.Property(d => d.CreatedAt)
            .IsRequired();

        builder.Property(d => d.UpdatedAt)
            .IsRequired(false);

        // Indexes for performance
        builder.HasIndex(d => new { d.IsFeatured, d.IsActive, d.ValidTo })
            .HasDatabaseName("IX_Deal_Featured_Active_ValidTo");

        builder.HasIndex(d => d.HotelId)
            .HasDatabaseName("IX_Deal_HotelId");

        builder.HasIndex(d => d.RoomTypeId)
            .HasDatabaseName("IX_Deal_RoomTypeId");

        builder.HasIndex(d => new { d.IsActive, d.ValidTo })
            .HasDatabaseName("IX_Deal_Active_ValidTo");

        // Unique constraints for data integrity
        builder.HasIndex(d => new { d.HotelId, d.RoomTypeId, d.ValidFrom, d.ValidTo })
            .IsUnique()
            .HasDatabaseName("IX_Deal_Hotel_RoomType_DateRange_Unique")
            .HasFilter("[IsActive] = 1");

        builder.HasIndex(d => new { d.HotelId, d.Title })
            .IsUnique()
            .HasDatabaseName("IX_Deal_Hotel_Title_Unique")
            .HasFilter("[IsActive] = 1");

        // Relationships
        builder.HasOne(d => d.Hotel)
            .WithMany()
            .HasForeignKey(d => d.HotelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.RoomType)
            .WithMany()
            .HasForeignKey(d => d.RoomTypeId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}