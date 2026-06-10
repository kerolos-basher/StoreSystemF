namespace Application.Products.Queries.SearchProductDetailsByBarcode;

public sealed record SearchProductDetailsByBarcodeQuery(string Barcode) : IQuery<ProductDetailsSearchDto?>;

public sealed record ProductDetailsSearchDto(
    long ProductDetailsId,
    long ProductId,
    string ProductName,
    string Barcode,
    decimal PurchasePrice,
    decimal SuggestedSellingPrice,
    string SupplierName,
    string CategoryName,
    int RemainingQuantity,
    string Notes);
