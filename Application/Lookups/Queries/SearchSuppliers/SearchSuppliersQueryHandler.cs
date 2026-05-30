using Application.Abstractions.Persistence;
using Application.Products.Dtos;

namespace Application.Lookups.Queries.SearchSuppliers;

public sealed class SearchSuppliersQueryHandler(IApplicationDbContext context)
    : IQueryHandler<SearchSuppliersQuery, IReadOnlyList<SupplierLookupDto>>
{
    public async Task<IReadOnlyList<SupplierLookupDto>> Handle(
        SearchSuppliersQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.Supplier.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Term))
        {
            var term = request.Term.Trim().ToLower();
            query = query.Where(s => s.Name.ToLower().Contains(term));
        }

        return await query
            .OrderBy(s => s.Name)
            .Take(20)
            .Select(s => new SupplierLookupDto(s.Id.ToString(), s.Name))
            .ToListAsync(cancellationToken);
    }
}
