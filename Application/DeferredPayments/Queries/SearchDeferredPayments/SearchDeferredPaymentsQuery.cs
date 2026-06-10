using Utilities.Response;

namespace Application.DeferredPayments.Queries.SearchDeferredPayments;

public sealed record SearchDeferredPaymentsQuery(
    string? CustomerTerm,
    bool? IsFullyPaid,
    int PageNumber = 1,
    int PageSize = 20) : IQuery<PagedResponse<DeferredPaymentListItemDto>>;

public sealed record DeferredPaymentListItemDto(
    long Id,
    long SalesInvoiceId,
    string InvoiceNumber,
    DateTime InvoiceDate,
    long CustomerId,
    string CustomerName,
    string CustomerPhone,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal RemainingAmount,
    bool IsFullyPaid);
