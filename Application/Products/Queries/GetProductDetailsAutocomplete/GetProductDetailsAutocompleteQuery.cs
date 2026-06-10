namespace Application.Products.Queries.GetProductDetailsAutocomplete;

public sealed record GetProductDetailsAutocompleteQuery(string Term) : IQuery<IReadOnlyList<ProductDetailsAutoCompleteDto>>;

public sealed record ProductDetailsAutoCompleteDto(
    long ProductDetailsId,
    string ProductName,
    string SupplierName,
    decimal SuggestedSellingPrice,
    decimal PurchasePrice,
    int RemainingQuantity,
    long ProductId,
    string Barcode);
