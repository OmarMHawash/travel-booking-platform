using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Infrastructure.Persistence.Configuration;

public class HotelImageConfiguration : IEntityTypeConfiguration<HotelImage>
{
    public void Configure(EntityTypeBuilder<HotelImage> builder)
    {
        builder.ToTable("HotelImage");
        builder.HasKey(hi => hi.Id);
        builder.Property(hi => hi.Url).IsRequired().HasMaxLength(500);
        builder.Property(hi => hi.Caption).HasMaxLength(200);
        builder.Property(hi => hi.SortOrder).IsRequired();
        builder.Property(hi => hi.IsCoverImage).IsRequired();

        builder.HasOne(hi => hi.Hotel)
            .WithMany(h => h.Images)
            .HasForeignKey(hi => hi.HotelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(hi => new { hi.HotelId, hi.SortOrder });
    }
}