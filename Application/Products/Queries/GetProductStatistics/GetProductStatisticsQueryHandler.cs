using Application.Abstractions.Persistence;
using Application.Products.Dtos;

namespace Application.Products.Queries.GetProductStatistics;

public sealed class GetProductStatisticsQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetProductStatisticsQuery, ProductStatisticsDto>
{
    public async Task<ProductStatisticsDto> Handle(
        GetProductStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        var totalProducts = await context.Product.AsNoTracking().CountAsync(cancellationToken);

        var aggregates = await context.ProductDetails
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalQuantity = g.Sum(x => x.RemainingQuantity),
                InventoryValue = g.Sum(x => x.RemainingQuantity * x.Price),
                LowStockCount = g.GroupBy(x => x.ProductId)
                    .Count(pg => pg.Sum(x => x.RemainingQuantity) <= request.LowStockThreshold)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return new ProductStatisticsDto(
            totalProducts,
            aggregates?.TotalQuantity ?? 0,
            aggregates?.LowStockCount ?? 0,
            aggregates?.InventoryValue ?? 0);
    }
}
