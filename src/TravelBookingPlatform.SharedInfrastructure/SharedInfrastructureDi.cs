using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TravelBookingPlatform.SharedInfrastructure.Persistence;
using TravelBookingPlatform.Core.Domain.Repositories;
using TravelBookingPlatform.SharedInfrastructure.Middleware;
using TravelBookingPlatform.SharedInfrastructure.Seeding;
using System.Reflection;
using TravelBookingPlatform.Core.Domain;

namespace TravelBookingPlatform.SharedInfrastructure;

public static class SharedInfrastructureDi
{
    public static IServiceCollection AddSharedInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] entityConfigurationAssemblies)
    {
        // Configure DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
            }
        );

        // Register the assemblies themselves for injection into DbContext
        services.AddSingleton(entityConfigurationAssemblies.AsEnumerable());

        // Register Unit of Work and base repository
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));

        // Register Database Seeder
        services.AddScoped<DatabaseSeeder>();

        // Add MediatR pipeline behaviors (e.g., for validation)
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Add Health Checks
        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();

        return services;
    }

    /// <summary>
    /// Configures the global exception handling middleware.
    /// </summary>
    /// <param name="app">The WebApplication instance.</param>
    /// <returns>The WebApplication instance for chaining.</returns>
    public static WebApplication UseGlobalExceptionHandling(this WebApplication app)
    {
        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
        return app;
    }
}
// Generic Validation Behavior for MediatR
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next(cancellationToken);
        }

        ValidationContext<TRequest> context = new ValidationContext<TRequest>(request);
        ValidationResult[] validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        List<ValidationFailure> failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

        if (failures.Count != 0)
        {
            // Throw a FluentValidation.ValidationException with the actual validation failures
            throw new FluentValidation.ValidationException(failures);
        }
        return await next(cancellationToken);
    }
}