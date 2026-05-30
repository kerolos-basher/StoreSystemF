namespace Domain.InventoryAggregate;

public sealed class InventoryTransaction : ParentEntity
{
    public long ProductId { get; private set; }
    public long? ProductDetailsId { get; private set; }
    public long? SalesInvoiceId { get; private set; }
    public int QuantityChange { get; private set; }
    public string TransactionType { get; private set; } = string.Empty;
    public DateTime TransactionDate { get; private set; }
    public string Reference { get; private set; } = string.Empty;

    private InventoryTransaction()
    {
    }

    public static InventoryTransaction CreateSale(
        long productId,
        long productDetailsId,
        long salesInvoiceId,
        int quantity,
        string reference)
    {
        if (quantity <= 0)
            throw new Exception("الكمية يجب أن تكون أكبر من صفر.");

        return new InventoryTransaction
        {
            ProductId = productId,
            ProductDetailsId = productDetailsId,
            SalesInvoiceId = salesInvoiceId,
            QuantityChange = -quantity,
            TransactionType = "Sale",
            TransactionDate = DateTime.UtcNow,
            Reference = reference
        };
    }
}
