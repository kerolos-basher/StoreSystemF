namespace Domain.InventoryAggregate;

public sealed class InventoryTransaction : ParentEntity
{
    public long ProductId { get; private set; }
    public long? ProductDetailsId { get; private set; }
    public long? SalesInvoiceId { get; private set; }
    public long? ReturnInvoiceId { get; private set; }
    public int QuantityChange { get; private set; }
    public InventoryTransactionType TransactionType { get; private set; }
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
            TransactionType = InventoryTransactionType.Sale,
            TransactionDate = DateTime.UtcNow,
            Reference = reference
        };
    }

    public static InventoryTransaction CreatePurchase(
        long productId,
        long productDetailsId,
        int quantity,
        string reference)
    {
        if (quantity <= 0)
            throw new Exception("الكمية يجب أن تكون أكبر من صفر.");

        return new InventoryTransaction
        {
            ProductId = productId,
            ProductDetailsId = productDetailsId,
            QuantityChange = quantity,
            TransactionType = InventoryTransactionType.Purchase,
            TransactionDate = DateTime.UtcNow,
            Reference = reference
        };
    }

    public static InventoryTransaction CreateCustomerReturn(
        long productId,
        long productDetailsId,
        long returnInvoiceId,
        int quantity,
        string reference)
    {
        if (quantity <= 0)
            throw new Exception("الكمية يجب أن تكون أكبر من صفر.");

        return new InventoryTransaction
        {
            ProductId = productId,
            ProductDetailsId = productDetailsId,
            ReturnInvoiceId = returnInvoiceId,
            QuantityChange = quantity,
            TransactionType = InventoryTransactionType.CustomerReturn,
            TransactionDate = DateTime.UtcNow,
            Reference = reference
        };
    }

    public static InventoryTransaction CreateDamagedReturn(
        long productId,
        long productDetailsId,
        long returnInvoiceId,
        string reference)
    {
        return new InventoryTransaction
        {
            ProductId = productId,
            ProductDetailsId = productDetailsId,
            ReturnInvoiceId = returnInvoiceId,
            QuantityChange = 0,
            TransactionType = InventoryTransactionType.Damaged,
            TransactionDate = DateTime.UtcNow,
            Reference = reference
        };
    }
}
