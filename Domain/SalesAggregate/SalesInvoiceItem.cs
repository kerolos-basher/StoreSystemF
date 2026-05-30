namespace Domain.SalesAggregate;

public sealed class SalesInvoiceItem : ParentEntity
{
    public long SalesInvoiceId { get; private set; }
    public long ProductId { get; private set; }
    public long ProductDetailsId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal => Quantity * UnitPrice;
    public string Notes { get; private set; } = string.Empty;

    private SalesInvoiceItem()
    {
    }

    internal SalesInvoiceItem(
        long salesInvoiceId,
        long productId,
        long productDetailsId,
        string productName,
        int quantity,
        decimal unitPrice,
        string notes)
    {
        SalesInvoiceId = salesInvoiceId;
        ProductId = productId;
        ProductDetailsId = productDetailsId;
        ProductName = productName.Trim();
        Quantity = quantity;
        UnitPrice = unitPrice;
        Notes = notes?.Trim() ?? string.Empty;
    }

    internal void IncreaseQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new Exception("الكمية غير صالحة.");

        Quantity += quantity;
    }
}
