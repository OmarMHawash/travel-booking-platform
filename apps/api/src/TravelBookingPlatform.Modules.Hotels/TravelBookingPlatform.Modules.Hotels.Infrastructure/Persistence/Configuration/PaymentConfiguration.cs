using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Infrastructure.Persistence.Configuration;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payment");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.BookingId).IsRequired();
        builder.Property(p => p.Amount).IsRequired().HasPrecision(18, 2);
        builder.Property(p => p.StripePaymentIntentId).IsRequired().HasMaxLength(255);
        builder.Property(p => p.PaymentMethod).IsRequired().HasMaxLength(50);
        builder.Property(p => p.Status).IsRequired();

        builder.HasIndex(p => p.BookingId).IsUnique();
        builder.HasIndex(p => p.StripePaymentIntentId).IsUnique();

        builder.HasOne(p => p.Booking)
            .WithOne()
            .HasForeignKey<Payment>(p => p.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(p => p.Amount)
            .IsRequired()
            .HasPrecision(18, 2);
    }
}