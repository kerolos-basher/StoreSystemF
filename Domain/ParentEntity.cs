using Domain.Events;

namespace Domain;

public abstract class ParentEntity : ISoftDelete
{
    public long Id { get; protected set; }
    public long CreatedBy { get; protected set; } = 0;
    public long? UpdatedBy { get; protected set; } = 0;
    public bool IsDeleted { get; protected set; } = false;

    public DateTime CreationTime { get; protected set; } = DateTime.Now;
    public DateTime? UpdateTime { get; protected set; }

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();

    protected static void EnsureValidId(long id)
    {
        if (id <= 0)
            throw new ArgumentException("المعرف غير صالح.", nameof(id));
    }

    protected void MarkUpdated(long? updatedBy = null)
    {
        UpdateTime = DateTime.Now;
        UpdatedBy = updatedBy;
    }
}
