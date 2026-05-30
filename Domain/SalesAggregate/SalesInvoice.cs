namespace Domain.SalesAggregate;

public sealed class SalesInvoice : ParentEntity
{
    public string InvoiceNumber { get; private set; } = string.Empty;
    public DateTime SaleDate { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal Discount { get; private set; }
    public decimal Tax { get; private set; }
    public decimal GrandTotal { get; private set; }
    public string Notes { get; private set; } = string.Empty;

    private readonly List<SalesInvoiceItem> _items = new();
    public IReadOnlyCollection<SalesInvoiceItem> Items => _items.AsReadOnly();

    private SalesInvoice()
    {
    }

    public static SalesInvoice Create(
        string invoiceNumber,
        decimal discount,
        decimal tax,
        string notes)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            throw new Exception("رقم الفاتورة مطلوب.");

        if (discount < 0)
            throw new Exception("الخصم لا يمكن أن يكون سالباً.");

        if (tax < 0)
            throw new Exception("الضريبة لا يمكن أن تكون سالبة.");

        return new SalesInvoice
        {
            InvoiceNumber = invoiceNumber.Trim(),
            SaleDate = DateTime.UtcNow,
            Discount = discount,
            Tax = tax,
            Notes = notes?.Trim() ?? string.Empty
        };
    }

    public void AddItem(
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
            x.ProductId == productId &&
            x.ProductDetailsId == productDetailsId &&
            x.UnitPrice == unitPrice);

        if (existing is not null)
        {
            existing.IncreaseQuantity(quantity);
            RecalculateTotals();
            return;
        }

        _items.Add(new SalesInvoiceItem(
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

    private void RecalculateTotals()
    {
        Subtotal = _items.Sum(x => x.LineTotal);
        GrandTotal = Math.Max(0, Subtotal - Discount + Tax);
    }
}
