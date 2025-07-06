using Microsoft.Extensions.DependencyInjection;
using TravelBookingPlatform.Modules.Identity.Application.Services;
using TravelBookingPlatform.Modules.Identity.Domain.Repositories;
using TravelBookingPlatform.Modules.Identity.Infrastructure.Persistence.Repositories;
using TravelBookingPlatform.Modules.Identity.Infrastructure.Services;

namespace TravelBookingPlatform.Modules.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<ITokenService, JwtTokenService>();

        return services;
    }
}