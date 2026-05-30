using Application.Abstractions.Persistence;
using Application.Products.Dtos;

namespace Application.Lookups.Queries.GetCategories;

public sealed class GetCategoriesQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetCategoriesQuery, IReadOnlyList<CategoryLookupDto>>
{
    public async Task<IReadOnlyList<CategoryLookupDto>> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        return await context.Category
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryLookupDto(c.Id, c.Name))
            .ToListAsync(cancellationToken);
    }
}
