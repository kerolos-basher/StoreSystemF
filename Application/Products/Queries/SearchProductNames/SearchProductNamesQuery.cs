using Application.Products.Dtos;

namespace Application.Products.Queries.SearchProductNames;

public sealed record SearchProductNamesQuery(string Term, int Limit = 10) : IQuery<IReadOnlyList<ProductNameLookupDto>>;
