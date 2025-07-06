using Microsoft.EntityFrameworkCore;
using System.Reflection;
using TravelBookingPlatform.Modules.Identity.Domain.Entities;

namespace TravelBookingPlatform.SharedInfrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    private readonly IEnumerable<Assembly> _entityConfigurationAssemblies;

    // Constructor now accepts an IEnumerable of Assemblies
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IEnumerable<Assembly> entityConfigurationAssemblies)
        : base(options)
    {
        _entityConfigurationAssemblies = entityConfigurationAssemblies ?? new List<Assembly>();
    }

    // Identity module entities
    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Dynamically apply configurations from all provided assemblies
        foreach (var assembly in _entityConfigurationAssemblies)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(assembly);
        }
    }
}