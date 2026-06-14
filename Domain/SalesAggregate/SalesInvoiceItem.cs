using Domain.Events;

namespace Domain.SalesAggregate;

public sealed class SalesInvoiceItem : ParentEntity
{
    public long SalesInvoiceId { get; private set; }
    public long ProductId { get; private set; }
    public long ProductDetailsId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public int ReturnedQuantity { get; private set; }
    public int AvailableForReturn => Quantity - ReturnedQuantity;
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal => Quantity * UnitPrice;
    public decimal NetLineTotal => (Quantity - ReturnedQuantity) * UnitPrice;
    public string Notes { get; private set; } = string.Empty;

    private SalesInvoiceItem()
    {
    }

    private SalesInvoiceItem(
        long id,
        long salesInvoiceId,
        long productId,
        long productDetailsId,
        string productName,
        int quantity,
        decimal unitPrice,
        string notes)
    {
        EnsureValidId(id);
        Id = id;
        SalesInvoiceId = salesInvoiceId;
        ProductId = productId;
        ProductDetailsId = productDetailsId;
        ProductName = productName.Trim();
        Quantity = quantity;
        ReturnedQuantity = 0;
        UnitPrice = unitPrice;
        Notes = notes?.Trim() ?? string.Empty;
    }

    internal static SalesInvoiceItem Create(
        long id,
        long salesInvoiceId,
        long productId,
        long productDetailsId,
        string productName,
        int quantity,
        decimal unitPrice,
        string notes) =>
        new(id, salesInvoiceId, productId, productDetailsId, productName, quantity, unitPrice, notes);

    internal void IncreaseQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new Exception("الكمية غير صالحة.");

        Quantity += quantity;
    }

    public void RegisterReturn(int quantity)
    {
        if (quantity <= 0)
            throw new Exception("الكمية غير صالحة.");

        if (quantity > AvailableForReturn)
            throw new Exception($"لا يمكن إرجاع أكثر من {AvailableForReturn} قطعة");

        ReturnedQuantity += quantity;
        AddDomainEvent(new ItemReturnedDomainEvent(Id, quantity));
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new Exception("الكمية يجب أن تكون أكبر من صفر.");

        if (newQuantity < ReturnedQuantity)
            throw new Exception("لا يمكن تقليل الكمية أقل من المرتجع.");

        Quantity = newQuantity;
    }

    public void UpdateUnitPrice(decimal unitPrice)
    {
        if (unitPrice <= 0)
            throw new Exception("سعر الوحدة يجب أن يكون أكبر من صفر.");

        UnitPrice = unitPrice;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
    }
}
