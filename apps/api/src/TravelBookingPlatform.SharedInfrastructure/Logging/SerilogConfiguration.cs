using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Events;

namespace TravelBookingPlatform.SharedInfrastructure.Logging;

public static class SerilogConfiguration
{
    private const string OutputTemplate =
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}";

    private const string BootstrapOutputTemplate =
        "[{Timestamp:HH:mm:ss} {Level:u3}] [Bootstrap] {Message:lj}{NewLine}{Exception}";
    
    public static WebApplicationBuilder ConfigureSerilog(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .WriteTo.Console(outputTemplate: OutputTemplate)
            .CreateLogger();
        
        builder.Host.UseSerilog(Log.Logger, dispose: true);
        
        return builder;
    }
    
    // Optional: Call this in Startup/Program.cs for initial logging
    public static void ConfigureBootstrapLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("System", LogEventLevel.Information)
            .MinimumLevel.Debug()
            .WriteTo.Console(outputTemplate: BootstrapOutputTemplate)
            .CreateLogger();
    }
}