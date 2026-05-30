using Application.Products.Dtos;

namespace Application.Products.Queries.GetPurchaseHistory;

public sealed record GetPurchaseHistoryQuery(long ProductId) : IQuery<IReadOnlyList<PurchaseHistoryItemDto>>;
