using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TravelBookingPlatform.Modules.Hotels.Infrastructure.Persistence.Repositories;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

namespace TravelBookingPlatform.Modules.Hotels.Infrastructure;

public static class HotelsInfrastructureDi
{
    public static IServiceCollection AddHotelsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ICityRepository, CityRepository>();
        return services;
    }
}