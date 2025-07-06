using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace TravelBookingPlatform.Modules.Hotels.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddHotelsApi(this IServiceCollection services)
    {
        // Any module-specific API registrations (e.g., custom formatters, policies)
        // For now, this module mainly exposes controllers which are discovered by the Host.
        // This method serves as a placeholder for future API-specific DI.
        return services;
    }

    public static IApplicationBuilder UseHotelsApi(this IApplicationBuilder app)
    {
        // Any module-specific API middleware (e.g., custom authentication per module)
        return app;
    }
}