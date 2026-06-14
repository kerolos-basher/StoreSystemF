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

    private InventoryTransaction(
        long id,
        long productId,
        long? productDetailsId,
        long? salesInvoiceId,
        long? returnInvoiceId,
        int quantityChange,
        InventoryTransactionType transactionType,
        string reference)
    {
        EnsureValidId(id);
        Id = id;
        ProductId = productId;
        ProductDetailsId = productDetailsId;
        SalesInvoiceId = salesInvoiceId;
        ReturnInvoiceId = returnInvoiceId;
        QuantityChange = quantityChange;
        TransactionType = transactionType;
        TransactionDate = DateTime.Now;
        Reference = reference;
    }

    public static InventoryTransaction CreateSale(
        long id,
        long productId,
        long productDetailsId,
        long salesInvoiceId,
        int quantity,
        string reference)
    {
        if (quantity <= 0)
            throw new Exception("الكمية يجب أن تكون أكبر من صفر.");

        return new InventoryTransaction(
            id,
            productId,
            productDetailsId,
            salesInvoiceId,
            null,
            -quantity,
            InventoryTransactionType.Sale,
            reference);
    }

    public static InventoryTransaction CreatePurchase(
        long id,
        long productId,
        long productDetailsId,
        int quantity,
        string reference)
    {
        if (quantity <= 0)
            throw new Exception("الكمية يجب أن تكون أكبر من صفر.");

        return new InventoryTransaction(
            id,
            productId,
            productDetailsId,
            null,
            null,
            quantity,
            InventoryTransactionType.Purchase,
            reference);
    }

    public static InventoryTransaction CreateCustomerReturn(
        long id,
        long productId,
        long productDetailsId,
        long returnInvoiceId,
        int quantity,
        string reference)
    {
        if (quantity <= 0)
            throw new Exception("الكمية يجب أن تكون أكبر من صفر.");

        return new InventoryTransaction(
            id,
            productId,
            productDetailsId,
            null,
            returnInvoiceId,
            quantity,
            InventoryTransactionType.CustomerReturn,
            reference);
    }

    public static InventoryTransaction CreateDamagedReturn(
        long id,
        long productId,
        long productDetailsId,
        long returnInvoiceId,
        string reference) =>
        new(
            id,
            productId,
            productDetailsId,
            null,
            returnInvoiceId,
            0,
            InventoryTransactionType.Damaged,
            reference);

    public static InventoryTransaction CreateSaleReversal(
        long id,
        long productId,
        long productDetailsId,
        long salesInvoiceId,
        int quantity,
        string reference)
    {
        if (quantity <= 0)
            throw new Exception("الكمية يجب أن تكون أكبر من صفر.");

        return new InventoryTransaction(
            id,
            productId,
            productDetailsId,
            salesInvoiceId,
            null,
            quantity,
            InventoryTransactionType.SaleReversal,
            reference);
    }
}
