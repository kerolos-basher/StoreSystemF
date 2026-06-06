namespace Domain.ReturnsAggregate;

public sealed class ReturnInvoice : ParentEntity
{
    public string ReturnNumber { get; private set; } = string.Empty;
    public long SalesInvoiceId { get; private set; }
    public DateTime ReturnDate { get; private set; }
    public decimal TotalAmount { get; private set; }
    public int ReturnReasonType { get; private set; }
    public string Notes { get; private set; } = string.Empty;

    private readonly List<ReturnInvoiceItem> _items = new();
    public IReadOnlyCollection<ReturnInvoiceItem> Items => _items.AsReadOnly();

    private ReturnInvoice()
    {
    }

    public static ReturnInvoice Create(
        string returnNumber,
        long salesInvoiceId,
        int returnReasonType,
        string notes)
    {
        if (string.IsNullOrWhiteSpace(returnNumber))
            throw new Exception("رقم المرتجع مطلوب.");

        if (salesInvoiceId <= 0)
            throw new Exception("معرف الفاتورة غير صالح.");

        if (returnReasonType <= 0)
            throw new Exception("سبب المرتجع مطلوب.");

        return new ReturnInvoice
        {
            ReturnNumber = returnNumber.Trim(),
            SalesInvoiceId = salesInvoiceId,
            ReturnDate = DateTime.UtcNow,
            ReturnReasonType = returnReasonType,
            Notes = notes?.Trim() ?? string.Empty
        };
    }

    public void AddItem(
        long salesInvoiceItemId,
        long productId,
        long productDetailsId,
        int quantity,
        decimal unitPrice,
        int itemReasonType,
        bool isReturnToStock,
        string notes)
    {
        if (quantity <= 0)
            throw new Exception("الكمية يجب أن تكون أكبر من صفر.");

        if (unitPrice <= 0)
            throw new Exception("سعر الوحدة يجب أن يكون أكبر من صفر.");

        _items.Add(new ReturnInvoiceItem(
            Id,
            salesInvoiceItemId,
            productId,
            productDetailsId,
            quantity,
            unitPrice,
            itemReasonType,
            isReturnToStock,
            notes));

        RecalculateTotal();
    }

    public void FinalizeReturn()
    {
        if (_items.Count == 0)
            throw new Exception("يجب أن يحتوي المرتجع على صنف واحد على الأقل.");

        RecalculateTotal();
    }

    private void RecalculateTotal()
    {
        TotalAmount = _items.Sum(x => x.LineTotal);
    }
}
