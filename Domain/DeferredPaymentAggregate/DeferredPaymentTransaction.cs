namespace Domain.DeferredPaymentAggregate;

public sealed class DeferredPaymentTransaction : ParentEntity
{
    public long DeferredPaymentId { get; private set; }
    public decimal AmountPaid { get; private set; }
    public DateTime PaymentDate { get; private set; }
    public string Notes { get; private set; } = string.Empty;

    private DeferredPaymentTransaction()
    {
    }

    private DeferredPaymentTransaction(
        long id,
        long deferredPaymentId,
        decimal amountPaid,
        string? notes,
        long createdBy)
    {
        EnsureValidId(id);
        Id = id;
        DeferredPaymentId = deferredPaymentId;
        AmountPaid = amountPaid;
        PaymentDate = DateTime.UtcNow;
        Notes = notes?.Trim() ?? string.Empty;
        CreatedBy = createdBy;
    }

    internal static DeferredPaymentTransaction Create(
        long id,
        long deferredPaymentId,
        decimal amountPaid,
        string? notes,
        long createdBy) =>
        new(id, deferredPaymentId, amountPaid, notes, createdBy);
}
