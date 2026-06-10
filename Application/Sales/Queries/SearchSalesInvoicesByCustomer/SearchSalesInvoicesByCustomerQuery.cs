using Application.Sales.Dtos;

namespace Application.Sales.Queries.SearchSalesInvoicesByCustomer;

public sealed record SearchSalesInvoicesByCustomerQuery(long CustomerId) : IQuery<IReadOnlyList<SalesInvoiceListItemDto>>;
