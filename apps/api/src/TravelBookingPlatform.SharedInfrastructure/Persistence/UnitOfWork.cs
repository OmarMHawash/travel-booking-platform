using Microsoft.EntityFrameworkCore;
using TravelBookingPlatform.Core.Domain;
using TravelBookingPlatform.Core.Domain.Entities;

namespace TravelBookingPlatform.SharedInfrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _dbContext;

    public UnitOfWork(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditProperties();
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    // automatically handle audit properties at the infrastructure level
    private void UpdateAuditProperties()
    {
        var entries = _dbContext.ChangeTracker.Entries<AggregateRoot>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                // CreatedAt is set in the constructor, so we don't need to set it here
            }
            else if (entry.State == EntityState.Modified)
            {
                // Use reflection to call MarkAsUpdated method
                var markAsUpdatedMethod = entry.Entity.GetType()
                    .GetMethod("MarkAsUpdated", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                markAsUpdatedMethod?.Invoke(entry.Entity, null);
            }
        }
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}