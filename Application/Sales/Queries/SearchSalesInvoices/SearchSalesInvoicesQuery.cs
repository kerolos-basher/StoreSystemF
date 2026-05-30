using Application.Sales.Dtos;
using Utilities.Response;

namespace Application.Sales.Queries.SearchSalesInvoices;

public sealed record SearchSalesInvoicesQuery(
    DateTime? DateFrom,
    DateTime? DateTo,
    string InvoiceNumber,
    int PageNumber,
    int PageSize) : IQuery<PagedResponse<SalesInvoiceListItemDto>>;
