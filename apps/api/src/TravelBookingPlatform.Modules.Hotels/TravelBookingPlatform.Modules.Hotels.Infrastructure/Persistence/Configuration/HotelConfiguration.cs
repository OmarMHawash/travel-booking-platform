using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Infrastructure.Persistence.Configuration;

public class HotelConfiguration : IEntityTypeConfiguration<Hotel>
{
    public void Configure(EntityTypeBuilder<Hotel> builder)
    {
        builder.ToTable("Hotel");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(h => h.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(h => h.Rating)
            .IsRequired()
            .HasColumnType("decimal(3,2)"); // e.g., 4.75

        builder.Property(h => h.CityId)
            .IsRequired();

        builder.Property(h => h.CreatedAt)
            .IsRequired();

        builder.Property(h => h.UpdatedAt)
            .IsRequired(false);

        // Indexes for performance
        builder.HasIndex(h => h.Name)
            .HasDatabaseName("IX_Hotel_Name");

        builder.HasIndex(h => h.CityId)
            .HasDatabaseName("IX_Hotel_CityId");

        builder.HasIndex(h => h.Rating)
            .HasDatabaseName("IX_Hotel_Rating");

        // Unique constraint for hotel name
        builder.HasIndex(h => h.Name)
            .IsUnique()
            .HasDatabaseName("IX_Hotel_Name_Unique");

        // Relationships
        builder.HasOne(h => h.City)
            .WithMany(c => c.Hotels)
            .HasForeignKey(h => h.CityId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent deleting city if hotels exist

        builder.HasMany(h => h.Rooms)
            .WithOne(r => r.Hotel)
            .HasForeignKey(r => r.HotelId)
            .OnDelete(DeleteBehavior.Cascade); // Delete all rooms when hotel is deleted
    }
}