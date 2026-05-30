using Application.Abstractions.Persistence;
using Application.Products.Dtos;
using Utilities.Response;

namespace Application.Products.Queries.SearchProducts;

public sealed class SearchProductsQueryHandler(IApplicationDbContext context)
    : IQueryHandler<SearchProductsQuery, PagedResponse<ProductListItemDto>>
{
    public async Task<PagedResponse<ProductListItemDto>> Handle(
        SearchProductsQuery request,
        CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        var rows = await context.ProductDetails
            .AsNoTracking()
            .Include(d => d.Product)
            .Include(d => d.Supplier)
            .Include(d => d.Category)
            .GroupBy(d => d.ProductId)
            .Select(g => new ProductRow(
                g.Key,
                g.First().Product.ProductName,
                g.First().Product.BarCode,
                g.Sum(x => x.RemainingQuantity),
                g.OrderByDescending(x => x.CreatedAt).Select(x => x.Price).FirstOrDefault(),
                g.OrderByDescending(x => x.CreatedAt).Select(x => x.SeLingPrice).FirstOrDefault(),
                g.OrderByDescending(x => x.CreatedAt).Select(x => x.Supplier != null ? x.Supplier.Name : string.Empty).FirstOrDefault(),
                g.OrderByDescending(x => x.CreatedAt).Select(x => x.Category != null ? x.Category.Name : string.Empty).FirstOrDefault(),
                g.Max(x => x.CreatedAt),
                g.Count(),
                g.Select(x => x.SupplierId).ToList(),
                g.Select(x => x.CategoryId).ToList()))
            .ToListAsync(cancellationToken);

        IEnumerable<ProductRow> filtered = rows;

        if (!string.IsNullOrWhiteSpace(request.ProductName))
        {
            var name = request.ProductName.Trim().ToLower();
            filtered = filtered.Where(x => x.ProductName.ToLower().Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(request.Barcode))
        {
            var barcode = request.Barcode.Trim().ToLower();
            filtered = filtered.Where(x => x.BarCode.ToString().ToLower().Contains(barcode));
        }

        if (request.SupplierId.HasValue)
            filtered = filtered.Where(x => x.SupplierIds.Contains(request.SupplierId.Value));

        if (request.CategoryId.HasValue)
            filtered = filtered.Where(x => x.CategoryIds.Contains(request.CategoryId.Value));

        if (request.PurchasePriceFrom.HasValue)
            filtered = filtered.Where(x => x.LatestPurchasePrice >= request.PurchasePriceFrom.Value);

        if (request.PurchasePriceTo.HasValue)
            filtered = filtered.Where(x => x.LatestPurchasePrice <= request.PurchasePriceTo.Value);

        if (request.SellingPriceFrom.HasValue)
            filtered = filtered.Where(x => x.SellingPrice >= request.SellingPriceFrom.Value);

        if (request.SellingPriceTo.HasValue)
            filtered = filtered.Where(x => x.SellingPrice <= request.SellingPriceTo.Value);

        if (request.QuantityFrom.HasValue)
            filtered = filtered.Where(x => x.CurrentQuantity >= request.QuantityFrom.Value);

        if (request.QuantityTo.HasValue)
            filtered = filtered.Where(x => x.CurrentQuantity <= request.QuantityTo.Value);

        filtered = ApplySorting(filtered, request.SortBy, request.SortDirection);

        var materialized = filtered.ToList();
        var totalCount = materialized.Count;
        var pageItems = materialized
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ProductListItemDto(
                x.ProductId,
                x.ProductName,
                x.BarCode.ToString(),
                x.CurrentQuantity,
                x.LatestPurchasePrice,
                x.SellingPrice,
                x.Supplier ?? string.Empty,
                x.Category ?? string.Empty,
                x.LastPurchaseDate,
                x.PurchaseLineCount,
                x.SupplierIds.Where(id => id.HasValue).Select(id => id!.Value).Distinct().Count()))
            .ToList();

        return new PagedResponse<ProductListItemDto>
        {
            Items = pageItems,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    private static IEnumerable<ProductRow> ApplySorting(
        IEnumerable<ProductRow> rows,
        string sortBy,
        string sortDirection)
    {
        var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        var key = (sortBy ?? string.Empty).Trim().ToLowerInvariant();

        return key switch
        {
            "productname" => descending
                ? rows.OrderByDescending(x => x.ProductName)
                : rows.OrderBy(x => x.ProductName),
            "currentquantity" => descending
                ? rows.OrderByDescending(x => x.CurrentQuantity)
                : rows.OrderBy(x => x.CurrentQuantity),
            "latestpurchaseprice" => descending
                ? rows.OrderByDescending(x => x.LatestPurchasePrice)
                : rows.OrderBy(x => x.LatestPurchasePrice),
            "sellingprice" => descending
                ? rows.OrderByDescending(x => x.SellingPrice)
                : rows.OrderBy(x => x.SellingPrice),
            "lastpurchasedate" => descending
                ? rows.OrderByDescending(x => x.LastPurchaseDate)
                : rows.OrderBy(x => x.LastPurchaseDate),
            _ => rows.OrderByDescending(x => x.LastPurchaseDate)
        };
    }

    private sealed record ProductRow(
        long ProductId,
        string ProductName,
        Guid BarCode,
        int CurrentQuantity,
        decimal LatestPurchasePrice,
        decimal SellingPrice,
        string Supplier,
        string Category,
        DateTime LastPurchaseDate,
        int PurchaseLineCount,
        List<long?> SupplierIds,
        List<long?> CategoryIds);
}
