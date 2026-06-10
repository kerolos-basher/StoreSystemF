namespace Domain.ReturnsAggregate;

public sealed class ReturnInvoiceItem : ParentEntity
{
    public long ReturnInvoiceId { get; private set; }
    public long SalesInvoiceItemId { get; private set; }
    public long ProductId { get; private set; }
    public long ProductDetailsId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal => Quantity * UnitPrice;
    public int ItemReasonType { get; private set; }
    public bool IsReturnToStock { get; private set; }
    public string Notes { get; private set; } = string.Empty;

    private ReturnInvoiceItem()
    {
    }

    private ReturnInvoiceItem(
        long id,
        long returnInvoiceId,
        long salesInvoiceItemId,
        long productId,
        long productDetailsId,
        int quantity,
        decimal unitPrice,
        int itemReasonType,
        bool isReturnToStock,
        string notes)
    {
        EnsureValidId(id);
        Id = id;
        ReturnInvoiceId = returnInvoiceId;
        SalesInvoiceItemId = salesInvoiceItemId;
        ProductId = productId;
        ProductDetailsId = productDetailsId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        ItemReasonType = itemReasonType;
        IsReturnToStock = isReturnToStock;
        Notes = notes?.Trim() ?? string.Empty;
    }

    internal static ReturnInvoiceItem Create(
        long id,
        long returnInvoiceId,
        long salesInvoiceItemId,
        long productId,
        long productDetailsId,
        int quantity,
        decimal unitPrice,
        int itemReasonType,
        bool isReturnToStock,
        string notes) =>
        new(id, returnInvoiceId, salesInvoiceItemId, productId, productDetailsId, quantity, unitPrice, itemReasonType, isReturnToStock, notes);
}
