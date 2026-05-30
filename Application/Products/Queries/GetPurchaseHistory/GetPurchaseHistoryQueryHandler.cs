using Application.Abstractions.Persistence;
using Application.Products.Dtos;

namespace Application.Products.Queries.GetPurchaseHistory;

public sealed class GetPurchaseHistoryQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetPurchaseHistoryQuery, IReadOnlyList<PurchaseHistoryItemDto>>
{
    public async Task<IReadOnlyList<PurchaseHistoryItemDto>> Handle(
        GetPurchaseHistoryQuery request,
        CancellationToken cancellationToken)
    {
        return await context.ProductDetails
            .AsNoTracking()
            .Include(d => d.Supplier)
            .Where(d => d.ProductId == request.ProductId)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new PurchaseHistoryItemDto(
                d.Price,
                d.Quantity,
                d.Supplier != null ? d.Supplier.Name : string.Empty,
                d.CreatedAt,
                d.Notes ?? string.Empty))
            .ToListAsync(cancellationToken);
    }
}
