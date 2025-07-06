using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TravelBookingPlatform.Modules.Hotels.Application.Mapping;
using System.Reflection;

namespace TravelBookingPlatform.Modules.Hotels.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddHotelsApplication(this IServiceCollection services)
    {

        // register MediatR Handlers from this assembly
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // AutoMapper is registered centrally in the Host project

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}