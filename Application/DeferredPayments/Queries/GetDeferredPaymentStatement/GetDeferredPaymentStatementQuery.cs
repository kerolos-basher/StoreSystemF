namespace Application.DeferredPayments.Queries.GetDeferredPaymentStatement;

public sealed record GetDeferredPaymentStatementQuery(long DeferredPaymentId) : IQuery<DeferredPaymentStatementDto>;

public sealed record DeferredPaymentStatementDto(
    long Id,
    long SalesInvoiceId,
    string InvoiceNumber,
    DateTime InvoiceDate,
    string CustomerName,
    string CustomerPhone,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal RemainingAmount,
    bool IsFullyPaid,
    IReadOnlyList<DeferredPaymentTransactionDto> Transactions);

public sealed record DeferredPaymentTransactionDto(
    long Id,
    decimal AmountPaid,
    DateTime PaymentDate,
    string Notes);
