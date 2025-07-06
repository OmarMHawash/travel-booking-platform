// System namespaces
using System.Reflection;
using System.Text;

// Microsoft namespaces
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

// Third-party namespaces
using Asp.Versioning;
using Serilog;

// Project-specific namespaces
using TravelBookingPlatform.Modules.Hotels.Api;
using TravelBookingPlatform.Modules.Hotels.Application;
using TravelBookingPlatform.Modules.Hotels.Infrastructure;
using TravelBookingPlatform.Modules.Identity.Api;
using TravelBookingPlatform.Modules.Identity.Infrastructure;
using TravelBookingPlatform.SharedInfrastructure;
using TravelBookingPlatform.SharedInfrastructure.Logging;
using TravelBookingPlatform.SharedInfrastructure.Persistence;

namespace TravelBookingPlatform.Host.Configuration;

public static class StartupExtensions
{
    /// <summary>
    /// Configures the application's services.
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder instance.</param>
    /// <returns>The WebApplicationBuilder instance for chaining.</returns>
    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
    {
        // 1. Configure Serilog
        builder.ConfigureSerilog();

        // 2. Add services to the container (new version)
        List<Assembly> entityConfigurationAssemblies =
        [
            typeof(HotelsInfrastructureDi).Assembly,
            typeof(TravelBookingPlatform.Modules.Identity.Infrastructure.DependencyInjection).Assembly // Identity Infrastructure assembly
            // Add other module infrastructure assemblies here as you create them:
        ];

        builder.Services.AddSharedInfrastructure(
            builder.Configuration,
            entityConfigurationAssemblies.ToArray());

        // Add API Controllers
        builder.Services.AddControllers();

        // Register AutoMapper with all module assemblies
        builder.Services.AddAutoMapper(cfg =>
        {
            cfg.AddMaps(typeof(TravelBookingPlatform.Modules.Hotels.Application.DependencyInjection).Assembly);
            cfg.AddMaps(typeof(TravelBookingPlatform.Modules.Identity.Application.DependencyInjection).Assembly);
        });

        builder.Services.AddHotelsApplication();
        builder.Services.AddHotelsInfrastructure(builder.Configuration);
        builder.Services.AddHotelsApi();

        // Add Identity module
        builder.Services.AddIdentityModule();

        // Configure JWT Authentication
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                    ValidAudience = builder.Configuration["JwtSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"] ?? "")),
                    ClockSkew = TimeSpan.Zero
                };
            });

        // Configure Authorization
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
            options.AddPolicy("UserPolicy", policy => policy.RequireAuthenticatedUser());
            options.AddPolicy("TypicalUserPolicy", policy => policy.RequireRole("TypicalUser"));
        });

        // Configure API Versioning
        builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        // Configure Swagger/OpenAPI
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            { Title = "Travel Booking Platform API", Version = "v1" });
            string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }

            // Add JWT Authentication to Swagger
            c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        // Add APIs problem Details 
        builder.Services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions["nodeId"] = Environment.MachineName;
            };
        });

        return builder; // Return builder for fluent chaining
    }

    /// <summary>
    /// Configures the application's HTTP request pipeline.
    /// </summary>
    /// <param name="app">The WebApplication instance.</param>
    /// <returns>The WebApplication instance for chaining.</returns>
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        // Configure the HTTP request pipeline.
        app.UseGlobalExceptionHandling();
        app.UseStatusCodePages();
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Travel Booking Platform API V1");
            });
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        // Authentication and Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        // Map Health Checks endpoint
        app.MapApplicationHealthCheckEndpoints();

        app.MapControllers();

        return app; // Return app for fluent chaining
    }

    /// <summary>
    /// Applies pending database migrations.
    /// </summary>
    /// <param name="app">The WebApplication instance.</param>
    public static async Task ApplyMigrations(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        IServiceProvider services = scope.ServiceProvider;
        try
        {
            ApplicationDbContext dbContext = services.GetRequiredService<ApplicationDbContext>();
            if ((await dbContext.Database.GetPendingMigrationsAsync()).Any())
            {
                Log.Information("Applying database migrations...");
                await dbContext.Database.MigrateAsync();
                Log.Information("Database migrations applied successfully.");
            }
            else
            {
                Log.Information("No pending database migrations to apply.");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while applying database migrations.");
        }
    }
}