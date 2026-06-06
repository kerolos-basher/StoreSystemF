using Application.Abstractions.Persistence;
using Application.Products.Dtos;

namespace Application.ReturnReasons.Queries.GetReturnReasons;

public sealed class GetReturnReasonsQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetReturnReasonsQuery, IReadOnlyList<ReturnReasonLookupDto>>
{
    public async Task<IReadOnlyList<ReturnReasonLookupDto>> Handle(
        GetReturnReasonsQuery request,
        CancellationToken cancellationToken)
    {
        return await context.ReturnReason
            .AsNoTracking()
            .OrderBy(r => r.Id)
            .Select(r => new ReturnReasonLookupDto(r.Id, r.Name, r.IsReturnToStock))
            .ToListAsync(cancellationToken);
    }
}
