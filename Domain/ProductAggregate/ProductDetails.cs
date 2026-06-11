using Domain.CategoryAggregate;
using Domain.Events;
using Domain.SupplierAggregate;

namespace Domain.ProductAggregate;

public class ProductDetails : ParentEntity
{
    public long ProductId { get; private set; }
    public long? SupplierId { get; private set; }
    public long? CategoryId { get; private set; }
    public string BarCode { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public decimal SeLingPrice { get; private set; }
    public int Quantity { get; private set; }
    public int RemainingQuantity { get; private set; }
    public int SoldQuantity => Quantity - RemainingQuantity;

    public string Notes { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public virtual Supplier Supplier { get; private set; } = default!;
    public virtual Product Product { get; private set; } = default!;
    public virtual Category Category { get; private set; } = default!;

    private ProductDetails()
    {
    }

    private ProductDetails(
        long id,
        long productId,
        long? supplierId,
        long? categoryId,
        decimal price,
        decimal sellingPrice,
        int quantity,
        string notes,
        string barCode,
        DateTime? purchaseDate)
    {
        EnsureValidId(id);
        Validate(price, sellingPrice, quantity);

        Id = id;
        ProductId = productId;
        SupplierId = supplierId;
        CategoryId = categoryId;
        BarCode = GenerateBarCode(barCode);
        Price = price;
        SeLingPrice = sellingPrice;
        Quantity = quantity;
        RemainingQuantity = quantity;
        Notes = notes ?? string.Empty;
        CreatedAt = purchaseDate ?? DateTime.UtcNow;
    }

    internal static ProductDetails Create(
        long id,
        long productId,
        long? supplierId,
        long? categoryId,
        decimal price,
        decimal sellingPrice,
        int quantity,
        string notes,
        string barCode,
        DateTime? purchaseDate) =>
        new ProductDetails(id, productId, supplierId, categoryId, price, sellingPrice, quantity, notes, barCode, purchaseDate);

    public void AddQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new Exception("Invalid quantity.");

        Quantity += quantity;
        RemainingQuantity += quantity;
        AddDomainEvent(new InventoryAddedDomainEvent(Id, quantity));
    }

    public void DeductStock(int quantity)
    {
        if (quantity <= 0)
            throw new StoreException("الكمية غير صالحة.");

        if (RemainingQuantity < quantity)
            throw new StoreException(string.Format(Resources.ExceptionMessage.InsufficientStock, quantity, RemainingQuantity));

        RemainingQuantity -= quantity;
        AddDomainEvent(new StockDeductedDomainEvent(Id, quantity));
    }

    public void ReduceStock(int quantity) => DeductStock(quantity);

    public void RestoreStock(int quantity)
    {
        if (quantity <= 0)
            throw new Exception("Invalid quantity.");

        if (SoldQuantity < quantity)
            throw new Exception("لا يمكن استرجاع كمية أكبر من المباعة.");

        RemainingQuantity += quantity;
    }

    public void UpdateNotes(string notes)
    {
        Notes = notes;
    }

    public void Update(
        long? supplierId,
        long? categoryId,
        decimal price,
        decimal sellingPrice,
        string notes)
    {
        Validate(price, sellingPrice, 1);

        if (SoldQuantity > 0 && (Price != price || SeLingPrice != sellingPrice))
            throw new Exception("لا يمكن تعديل الأسعار لوجود مبيعات على هذه الدفعة.");

        SupplierId = supplierId;
        CategoryId = categoryId;
        Price = price;
        SeLingPrice = sellingPrice;
        Notes = notes ?? string.Empty;
        MarkUpdated();
    }

    public void AssignBarCode(long sequenceValue)
    {
        if (string.IsNullOrWhiteSpace(BarCode) || BarCode.StartsWith("TEMP"))
            BarCode = $"BC{sequenceValue:D10}";
    }

    private static string GenerateBarCode(string? barCode)
    {
        if (!string.IsNullOrWhiteSpace(barCode))
            return barCode.Trim();

        return $"TEMP{DateTime.UtcNow.Ticks % 1000000000:D9}";
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        MarkUpdated();
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new Exception("الكمية يجب أن تكون أكبر من صفر.");

        if (SoldQuantity > 0 && quantity < Quantity)
            throw new Exception("لا يمكن تقليل الكمية لوجود مبيعات.");

        var diff = quantity - Quantity;
        Quantity = quantity;
        RemainingQuantity += diff;
    }

    private static void Validate(
        decimal price,
        decimal sellingPrice,
        int quantity)
    {
        if (price <= 0)
            throw new Exception("Purchase price must be greater than zero.");

        if (sellingPrice <= 0)
            throw new Exception("Selling price must be greater than zero.");

        if (quantity <= 0)
            throw new Exception("Quantity must be greater than zero.");
    }
}
