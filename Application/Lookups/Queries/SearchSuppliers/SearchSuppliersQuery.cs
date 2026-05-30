using Application.Products.Dtos;

namespace Application.Lookups.Queries.SearchSuppliers;

public sealed record SearchSuppliersQuery(string Term) : IQuery<IReadOnlyList<SupplierLookupDto>>;
