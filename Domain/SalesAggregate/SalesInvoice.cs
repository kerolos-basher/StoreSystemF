using Domain.Events;

namespace Domain.SalesAggregate;

public sealed class SalesInvoice : ParentEntity
{
    public string InvoiceNumber { get; private set; } = string.Empty;
    public DateTime SaleDate { get; private set; }
    public long? CustomerId { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal GrandTotal { get; private set; }
    public string Notes { get; private set; } = string.Empty;
    public bool IsDeferredPayment { get; private set; }

    private readonly List<SalesInvoiceItem> _items = new();
    public IReadOnlyCollection<SalesInvoiceItem> Items => _items.AsReadOnly();

    private SalesInvoice()
    {
    }

    private SalesInvoice(
        long id,
        string notes,
        long? customerId,
        bool isDeferredPayment)
    {
        EnsureValidId(id);
        Id = id;
        InvoiceNumber = $"INV-{id}";
        SaleDate = DateTime.Now;
        CustomerId = customerId;
        Notes = notes?.Trim() ?? string.Empty;
        IsDeferredPayment = isDeferredPayment;
    }

    public static SalesInvoice Create(
        long id,
        string notes,
        long? customerId = null,
        bool isDeferredPayment = false) =>
        new(id, notes, customerId, isDeferredPayment);

    public void AddItem(
        long itemId,
        long productId,
        long productDetailsId,
        string productName,
        int quantity,
        decimal unitPrice,
        string notes)
    {
        if (quantity <= 0)
            throw new Exception("الكمية يجب أن تكون أكبر من صفر.");

        if (unitPrice <= 0)
            throw new Exception("سعر الوحدة يجب أن يكون أكبر من صفر.");

        var existing = _items.FirstOrDefault(x =>
            !x.IsDeleted &&
            x.ProductId == productId &&
            x.ProductDetailsId == productDetailsId &&
            x.UnitPrice == unitPrice);

        if (existing is not null)
        {
            existing.IncreaseQuantity(quantity);
            RecalculateTotals();
            return;
        }

        _items.Add(SalesInvoiceItem.Create(
            itemId,
            Id,
            productId,
            productDetailsId,
            productName,
            quantity,
            unitPrice,
            notes));

        RecalculateTotals();
    }

    public void FinalizeInvoice()
    {
        if (_items.Count == 0)
            throw new Exception("يجب أن تحتوي الفاتورة على صنف واحد على الأقل.");

        RecalculateTotals();
    }

    public void UpdateNotes(string notes, bool isDeferredPayment)
    {
        Notes = notes?.Trim() ?? string.Empty;
        IsDeferredPayment = isDeferredPayment;
        MarkUpdated();
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        foreach (var item in _items)
            item.SoftDelete();
        AddDomainEvent(new InvoiceDeletedDomainEvent(Id));
        MarkUpdated();
    }

    public void RecalculateAfterReturn()
    {
        foreach (var item in _items.Where(x => !x.IsDeleted && x.AvailableForReturn <= 0))
            item.SoftDelete();

        RecalculateTotals();
        MarkUpdated();
    }

    private void RecalculateTotals()
    {
        Subtotal = _items.Where(x => !x.IsDeleted).Sum(x => x.NetLineTotal);
        GrandTotal = Subtotal;
    }
}
