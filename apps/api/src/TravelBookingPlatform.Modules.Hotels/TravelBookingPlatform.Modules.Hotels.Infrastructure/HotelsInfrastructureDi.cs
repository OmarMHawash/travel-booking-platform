using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TravelBookingPlatform.Modules.Hotels.Application.Interfaces;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;
using TravelBookingPlatform.Modules.Hotels.Infrastructure.Persistence.Repositories;
using TravelBookingPlatform.Modules.Hotels.Infrastructure.Services;

namespace TravelBookingPlatform.Modules.Hotels.Infrastructure;

public static class HotelsInfrastructureDi
{
    public static IServiceCollection AddHotelsInfrastructure(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        // Register all repositories
        services.AddScoped<ICityRepository, CityRepository>();
        services.AddScoped<IHotelRepository, HotelRepository>();
        services.AddScoped<IRoomTypeRepository, RoomTypeRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IDealRepository, DealRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.Configure<SendGridSettings>(configuration.GetSection("SendGrid"));
        services.AddScoped<IEmailService, SendGridEmailService>();
        services.AddScoped<IPdfService, QuestPdfService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        if (environment.IsDevelopment())
        {
            services.AddScoped<IPaymentGatewayService, FakePaymentGatewayService>();
        }
        else
        {
            services.Configure<StripeSettings>(configuration.GetSection("Stripe"));
            services.AddScoped<IPaymentGatewayService, StripeService>();
        }

        return services;
    }
}