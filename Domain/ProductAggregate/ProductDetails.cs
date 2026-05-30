
using Domain.CategoryAggregate;
using Domain.SupplierAggregate;

namespace Domain.ProductAggregate;

public class ProductDetails : ParentEntity
{
    public long ProductId { get; private set; }
    public long? SupplierId { get; private set; }
    public long? CategoryId { get; private set; }
    public decimal Price { get; private set; }
    public decimal SeLingPrice { get; private set; }
    public int Quantity { get; private set; }
    public int RemainingQuantity { get; private set; }
    public int SoldQuantity =>
        Quantity - RemainingQuantity;

    public string Notes { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public virtual Supplier Supplier { get; private set; } = default!;
    public virtual Product Product { get; private set; } = default!;
    public virtual Category Category { get; private set; } = default!;

    private ProductDetails()
    {
    }

    public ProductDetails(long productId,
        long? supplierId, long? categoryId, decimal price,
        decimal sellingPrice, int quantity, string notes,
        DateTime? purchaseDate = null)
    {
        ProductId = productId;
        SupplierId = supplierId;
        CategoryId = categoryId;
        Price = price;
        SeLingPrice = sellingPrice;
        Quantity = quantity;
        RemainingQuantity = quantity;
        Notes = notes ?? string.Empty;
        CreatedAt = purchaseDate ?? DateTime.UtcNow;
    }

    internal static ProductDetails Create(
    long productId,
    long? supplierId,
    long? categoryId,
    decimal price,
    decimal sellingPrice,
    int quantity,
    string notes)
    {
        Validate(price, sellingPrice, quantity);

        return new ProductDetails(productId, supplierId,
            categoryId, price, sellingPrice, quantity, notes);
    }

    public void AddQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new Exception("Invalid quantity.");

        Quantity += quantity;
        RemainingQuantity += quantity;
    }

    public void ReduceStock(int quantity)
    {
        if (quantity <= 0)
            throw new Exception("Invalid quantity.");

        if (RemainingQuantity < quantity)
            throw new Exception("Insufficient stock.");

        RemainingQuantity -= quantity;
    }

    public void UpdateNotes(string notes)
    {
        Notes = notes;
    }

    private static void Validate(
        decimal price,
        decimal sellingPrice,
        int quantity)
    {
        if (price <= 0)
            throw new Exception(
                "Purchase price must be greater than zero.");

        if (sellingPrice <= 0)
            throw new Exception(
                "Selling price must be greater than zero.");

        if (quantity <= 0)
            throw new Exception(
                "Quantity must be greater than zero.");
    }
}