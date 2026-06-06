using Application.Returns.Dtos;
using Utilities.Response;

namespace Application.Returns.Queries.SearchReturnInvoices;

public sealed record SearchReturnInvoicesQuery(
    DateTime? DateFrom,
    DateTime? DateTo,
    string ReturnNumber,
    int PageNumber,
    int PageSize) : IQuery<PagedResponse<ReturnInvoiceListItemDto>>;
