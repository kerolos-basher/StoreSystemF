namespace Application.Products.Dtos;

public sealed record CreatePurchaseEntryResultDto(string ProductId);

public sealed record ProductListItemDto(
    string Id,
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
    string Id,
    string Supplier,
    string Category,
    decimal PurchasePrice,
    decimal SellingPrice,
    int Quantity,
    int RemainingQuantity,
    DateTime PurchaseDate,
    string Notes);

public sealed record ProductDetailsDto(
    string Id,
    string ProductName,
    string Barcode,
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

public sealed record CategoryLookupDto(string Id, string Name);

public sealed record SupplierLookupDto(string Id, string Name);

public sealed record ProductByBarcodeDto(
    string Id,
    string ProductName,
    string Barcode,
    decimal SellingPrice,
    int AvailableQuantity,
    string ImageUrl);

public sealed record QRCodeDto(
    string ProductId,
    string Barcode,
    string Base64Image,
    string ContentType);
