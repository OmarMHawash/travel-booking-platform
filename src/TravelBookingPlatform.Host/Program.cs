using TravelBookingPlatform.Host.Configuration;
using Serilog;
using TravelBookingPlatform.SharedInfrastructure.Logging;

try
{
    SerilogConfiguration.ConfigureBootstrapLogger();
    Log.Information("Starting application build process...");

    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
    builder.ConfigureServices();

    WebApplication app = builder.Build();
    app.ConfigurePipeline();
    
    // Uncomment the line below if you want to apply migrations on startup (e.g., for dev/testing)
    if (app.Environment.IsDevelopment())
    {
        await app.ApplyMigrations();
    }

    Log.Information("Starting web host");
    app.Run();
}
catch (Exception ex)
{
    if (ex is not HostAbortedException)
    {
        Log.Fatal(ex, "Application terminated unexpectedly");
    }
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }