using Microsoft.Extensions.DependencyInjection;
using TravelBookingPlatform.Modules.Identity.Application;
using TravelBookingPlatform.Modules.Identity.Infrastructure;

namespace TravelBookingPlatform.Modules.Identity.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services)
    {
        services.AddIdentityApplication();
        services.AddIdentityInfrastructure();

        return services;
    }
}