namespace Application.Customers.Queries.GetCustomersAutocomplete;

public sealed record GetCustomersAutocompleteQuery(string Term) : IQuery<IReadOnlyList<CustomerAutoCompleteDto>>;

public sealed record CustomerAutoCompleteDto(long Id, string Name, string Phone);
