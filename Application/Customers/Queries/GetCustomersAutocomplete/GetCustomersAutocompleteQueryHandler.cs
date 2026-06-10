using Application.Abstractions.Persistence;

namespace Application.Customers.Queries.GetCustomersAutocomplete;

public sealed class GetCustomersAutocompleteQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetCustomersAutocompleteQuery, IReadOnlyList<CustomerAutoCompleteDto>>
{
    public async Task<IReadOnlyList<CustomerAutoCompleteDto>> Handle(
        GetCustomersAutocompleteQuery request,
        CancellationToken cancellationToken)
    {
        var term = request.Term?.Trim() ?? string.Empty;
        if (term.Length < 2)
            return [];

        var lowered = term.ToLower();

        return await context.Customer
            .AsNoTracking()
            .Where(c => c.Name.ToLower().Contains(lowered) || c.Phone.Contains(term))
            .OrderBy(c => c.Name)
            .Take(10)
            .Select(c => new CustomerAutoCompleteDto(c.Id, c.Name, c.Phone))
            .ToListAsync(cancellationToken);
    }
}
