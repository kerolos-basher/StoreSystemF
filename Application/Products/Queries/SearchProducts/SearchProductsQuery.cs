using Application.Products.Dtos;
using Utilities.Response;

namespace Application.Products.Queries.SearchProducts;

public sealed record SearchProductsQuery(
    string ProductName,
    string Barcode,
    long? SupplierId,
    long? CategoryId,
    decimal? PurchasePriceFrom,
    decimal? PurchasePriceTo,
    decimal? SellingPriceFrom,
    decimal? SellingPriceTo,
    int? QuantityFrom,
    int? QuantityTo,
    string SortBy,
    string SortDirection,
    int PageNumber,
    int PageSize) : IQuery<PagedResponse<ProductListItemDto>>;
