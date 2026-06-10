using Domain.Events;

namespace Domain.ProductAggregate;

public sealed class Product : ParentEntity
{
    public string ProductName { get; private set; } = string.Empty;

    private readonly List<ProductDetails> _productDetails = new();

    public IReadOnlyCollection<ProductDetails> ProductDetails => _productDetails.AsReadOnly();

    public int TotalQuantity => _productDetails.Sum(x => x.RemainingQuantity);

    private Product()
    {
    }

    private Product(long id, string productName)
    {
        EnsureValidId(id);
        Id = id;
        ProductName = productName;
    }

    public static Product Create(
        long id,
        long productDetailsId,
        string productName,
        long? supplierId,
        long? categoryId,
        decimal purchasePrice,
        decimal sellingPrice,
        int quantity,
        string notes,
        string? barCode = null,
        DateTime? purchaseDate = null)
    {
        ValidateProduct(productName);

        var product = new Product(id, productName.Trim());
        product.AddOrUpdateDetails(
            productDetailsId,
            supplierId,
            categoryId,
            purchasePrice,
            sellingPrice,
            quantity,
            notes,
            barCode,
            purchaseDate);

        return product;
    }

    public void AddOrUpdateDetails(
        long productDetailsId,
        long? supplierId,
        long? categoryId,
        decimal purchasePrice,
        decimal sellingPrice,
        int quantity,
        string notes,
        string? barCode = null,
        DateTime? purchaseDate = null)
    {
        if (quantity <= 0)
            throw new Exception("Quantity must be greater than zero.");

        var existingDetails = _productDetails.FirstOrDefault(x =>
            !x.IsDeleted &&
            x.SupplierId == supplierId &&
            x.CategoryId == categoryId &&
            x.Price == purchasePrice &&
            x.SeLingPrice == sellingPrice);

        if (existingDetails is not null)
        {
            existingDetails.AddQuantity(quantity);
            return;
        }

        _productDetails.Add(global::Domain.ProductAggregate.ProductDetails.Create(
            productDetailsId,
            Id,
            supplierId,
            categoryId,
            purchasePrice,
            sellingPrice,
            quantity,
            notes,
            barCode ?? string.Empty,
            purchaseDate));
    }

    public void Update(string productName)
    {
        ValidateProduct(productName);
        ProductName = productName.Trim();
        MarkUpdated();
    }

    public void Delete()
    {
        if (_productDetails.Any(pd => !pd.IsDeleted))
            throw new Exception("لا يمكن حذف الصنف قبل حذف جميع الدفعات");

        IsDeleted = true;
        AddDomainEvent(new ProductDeletedDomainEvent(Id));
        MarkUpdated();
    }

    public void SoftDelete() => Delete();

    public void DeleteDetails(long detailsId, bool forceDelete = false)
    {
        var details = _productDetails
            .FirstOrDefault(x => x.Id == detailsId && !x.IsDeleted);

        if (details is null)
            throw new Exception("التفاصيل غير موجودة.");

        if (details.RemainingQuantity > 0 && !forceDelete)
            throw new Exception($"المتبقي من هذه الدفعة {details.RemainingQuantity} قطعة");

        details.SoftDelete();
        AddDomainEvent(new InventoryRemovedDomainEvent(detailsId, details.RemainingQuantity));
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

    public StockAllocation ReduceStockFromDetails(long productDetailsId, int quantity)
    {
        if (quantity <= 0)
            throw new Exception("الكمية يجب أن تكون أكبر من صفر.");

        if (IsDeleted)
            throw new Exception("لا يمكن بيع منتج محذوف.");

        var detail = _productDetails.FirstOrDefault(x => x.Id == productDetailsId)
            ?? throw new Exception("تفاصيل المنتج غير موجودة.");

        if (detail.RemainingQuantity < quantity)
            throw new Exception("الكمية المتاحة غير كافية.");

        detail.ReduceStock(quantity);
        return new StockAllocation(detail.Id, quantity, detail.SeLingPrice);
    }

    public void RestoreStock(long productDetailsId, int quantity)
    {
        if (quantity <= 0)
            throw new Exception("الكمية يجب أن تكون أكبر من صفر.");

        var detail = _productDetails.FirstOrDefault(x => x.Id == productDetailsId)
            ?? throw new Exception("تفاصيل المنتج غير موجودة.");

        detail.RestoreStock(quantity);
    }

    private static void ValidateProduct(string productName)
    {
        if (string.IsNullOrWhiteSpace(productName))
            throw new Exception("اسم المنتج مطلوب.");
    }
}
