namespace Application.Products.Queries.GetProductsAutocomplete;

public sealed record GetProductsAutocompleteQuery(string Term) : IQuery<IReadOnlyList<ProductAutoCompleteDto>>;

public sealed record ProductAutoCompleteDto(long Id, string ProductName);
