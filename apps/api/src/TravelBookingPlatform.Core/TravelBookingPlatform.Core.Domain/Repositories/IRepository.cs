using TravelBookingPlatform.Core.Domain.Entities;
using System.Linq.Expressions;

namespace TravelBookingPlatform.Core.Domain.Repositories;

// Generic interface for all repositories
public interface IRepository<TEntity> where TEntity : AggregateRoot
{
    Task<TEntity?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<TEntity>> GetAllAsync();
    Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
    Task AddAsync(TEntity entity);
    void Update(TEntity entity);
    void Delete(TEntity entity);
}