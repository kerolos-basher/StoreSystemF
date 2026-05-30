using Application.Products.Dtos;

namespace Application.Lookups.Queries.GetCategories;

public sealed record GetCategoriesQuery() : IQuery<IReadOnlyList<CategoryLookupDto>>;
