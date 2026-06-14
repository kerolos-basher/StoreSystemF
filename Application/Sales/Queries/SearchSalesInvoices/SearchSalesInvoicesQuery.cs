using Application.Sales.Dtos;
using Utilities.Response;

namespace Application.Sales.Queries.SearchSalesInvoices;

public sealed record SearchSalesInvoicesQuery(
    DateTime? DateFrom,
    DateTime? DateTo,
    string InvoiceNumber,
    string? CustomerTerm,
    string? ProductName,
    bool? IsDeferredPayment,
    int PageNumber,
    int PageSize) : IQuery<PagedResponse<SalesInvoiceListItemDto>>;
