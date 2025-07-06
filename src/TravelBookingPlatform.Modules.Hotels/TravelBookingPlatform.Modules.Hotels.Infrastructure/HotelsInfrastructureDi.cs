using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TravelBookingPlatform.Modules.Hotels.Infrastructure.Persistence.Repositories;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

namespace TravelBookingPlatform.Modules.Hotels.Infrastructure;

public static class HotelsInfrastructureDi // Renamed earlier
{
    public static IServiceCollection AddHotelsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ICityRepository, CityRepository>();
        return services;
    }
        
    // Not needed for centralized DbContext
    // If you keep it, make sure it refers to its own assembly, but it won't be called directly for DbContext config.
    // public static ModelBuilder ApplyHotelsConfigurations(this ModelBuilder modelBuilder)
    // {
    //     modelBuilder.ApplyConfigurationsFromAssembly(typeof(HotelsInfrastructureDi).Assembly);
    //     return modelBuilder;
    // }
}