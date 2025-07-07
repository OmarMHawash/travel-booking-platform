using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TravelBookingPlatform.Modules.Hotels.Infrastructure.Persistence.Repositories;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

namespace TravelBookingPlatform.Modules.Hotels.Infrastructure;

public static class HotelsInfrastructureDi
{
    public static IServiceCollection AddHotelsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register all repositories
        services.AddScoped<ICityRepository, CityRepository>();
        services.AddScoped<IHotelRepository, HotelRepository>();
        services.AddScoped<IRoomTypeRepository, RoomTypeRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IDealRepository, DealRepository>();

        return services;
    }
}