namespace Application.Products.Dtos;

public sealed record CreatePurchaseEntryResultDto(long ProductId, long? ProductDetailsId, string Barcode);

public sealed record ProductNameLookupDto(long Id, string ProductName);

public sealed record ProductListItemDto(
    long Id,
    string ProductName,
    string Barcode,
    int CurrentQuantity,
    decimal LatestPurchasePrice,
    decimal SellingPrice,
    string Supplier,
    string Category,
    DateTime? LastPurchaseDate,
    int PurchaseLineCount,
    int SupplierCount);

public sealed record ProductDetailLineDto(
    long Id,
    string Barcode,
    long? SupplierId,
    string Supplier,
    long? CategoryId,
    string Category,
    decimal PurchasePrice,
    decimal SellingPrice,
    int Quantity,
    int RemainingQuantity,
    DateTime PurchaseDate,
    string Notes);

public sealed record ProductDetailsDto(
    long Id,
    string ProductName,
    int TotalQuantity,
    decimal InventoryValue,
    int PurchaseLineCount,
    int SupplierCount,
    IReadOnlyList<ProductDetailLineDto> Lines);

public sealed record PurchaseHistoryItemDto(
    decimal PurchasePrice,
    int Quantity,
    string Supplier,
    DateTime PurchaseDate,
    string Notes);

public sealed record ProductStatisticsDto(
    int TotalProducts,
    int TotalQuantity,
    int LowStockCount,
    decimal InventoryValue);

public sealed record CategoryLookupDto(long Id, string Name);

public sealed record SupplierLookupDto(long Id, string Name);

public sealed record ReturnReasonLookupDto(long Id, string Name, bool IsReturnToStock);

public sealed record ProductByBarcodeDto(
    long Id,
    long ProductDetailsId,
    string ProductName,
    string Barcode,
    decimal SellingPrice,
    int AvailableQuantity,
    string ImageUrl);

public sealed record QRCodeDto(
    string ProductId,
    string ProductDetailsId,
    string Barcode,
    string Base64Image,
    string ContentType);
