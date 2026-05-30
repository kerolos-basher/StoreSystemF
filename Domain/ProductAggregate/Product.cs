

namespace Domain.ProductAggregate;

public sealed class Product : ParentEntity
{
    public string ProductName { get; private set; } = string.Empty;
    public Guid BarCode { get; private set; }

    private readonly List<ProductDetails> _productDetails = new();

    public IReadOnlyCollection<ProductDetails> ProductDetails => _productDetails.AsReadOnly();

    public int TotalQuantity => _productDetails.Sum(x => x.RemainingQuantity);

    private Product()
    {
    }

    private Product(
        string productName,
        Guid barCode)
    {
        ProductName = productName;
        BarCode = barCode;
    }

    public static Product Create(
        string productName,
        Guid? barCode,
        long? supplierId,
        long? categoryId,
        decimal purchasePrice,
        decimal sellingPrice,
        int quantity,
        string notes,
        DateTime? purchaseDate = null)
    {
        ValidateProduct(productName);

        var product = new Product(
            productName.Trim(),
            barCode ?? Guid.NewGuid());

        product.AddOrUpdateDetails(
            supplierId,
            categoryId,
            purchasePrice,
            sellingPrice,
            quantity,
            notes,
            purchaseDate);

        return product;
    }

    public void AddOrUpdateDetails(
        long? supplierId,
        long? categoryId,
        decimal purchasePrice,
        decimal sellingPrice,
        int quantity,
        string notes,
        DateTime? purchaseDate = null)
    {
        if (quantity <= 0)
            throw new Exception("Quantity must be greater than zero.");

        var existingDetails = _productDetails.FirstOrDefault(x =>
            x.SupplierId == supplierId &&
            x.CategoryId == categoryId &&
            x.Price == purchasePrice &&
            x.SeLingPrice == sellingPrice);

        if (existingDetails is not null)
        {
            existingDetails.AddQuantity(quantity);
            return;
        }

        var details = new ProductDetails(
            Id,
            supplierId,
            categoryId,
            purchasePrice,
            sellingPrice,
            quantity,
            notes,
            purchaseDate);

        _productDetails.Add(details);
    }

    public void Update(
        string productName,
        Guid barCode)
    {
        ValidateProduct(productName);

        ProductName = productName.Trim();
        BarCode = barCode;
    }

    public void DeleteDetails(long detailsId)
    {
        var details = _productDetails
            .FirstOrDefault(x => x.Id == detailsId);

        if (details is null)
            throw new Exception("التفاصيل غير موجودة.");

        if (details.SoldQuantity > 0)
            throw new Exception("لا يمكن حذف التفاصيل لوجود مبيعات.");

        _productDetails.Remove(details);
    }

    public IReadOnlyList<StockAllocation> ReduceStockFifo(int quantity)
    {
        if (quantity <= 0)
            throw new Exception("الكمية يجب أن تكون أكبر من صفر.");

        if (IsDeleted)
            throw new Exception("لا يمكن بيع منتج محذوف.");

        var available = _productDetails.Sum(x => x.RemainingQuantity);
        if (available < quantity)
            throw new Exception("الكمية المتاحة غير كافية.");

        var remaining = quantity;
        var allocations = new List<StockAllocation>();

        foreach (var detail in _productDetails
                     .Where(x => x.RemainingQuantity > 0)
                     .OrderBy(x => x.CreatedAt))
        {
            var take = Math.Min(remaining, detail.RemainingQuantity);
            detail.ReduceStock(take);
            allocations.Add(new StockAllocation(detail.Id, take, detail.SeLingPrice));
            remaining -= take;

            if (remaining == 0)
                break;
        }

        return allocations;
    }

    private static void ValidateProduct(string productName)
    {
        if (string.IsNullOrWhiteSpace(productName))
            throw new Exception("اسم المنتج مطلوب.");
    }
}
