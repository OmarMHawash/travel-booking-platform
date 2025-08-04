using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace TravelBookingPlatform.Modules.Identity.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        // AutoMapper is registered centrally in the Host project

        return services;
    }
}