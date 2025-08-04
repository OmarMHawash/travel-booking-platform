namespace TravelBookingPlatform.Core.Domain.Entities;

public abstract class AggregateRoot
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }

    protected AggregateRoot()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.Now;
    }

    protected void MarkAsUpdated()
    {
        UpdatedAt = DateTime.Now;
    }
}