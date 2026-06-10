using Application.Abstractions.Persistence;

namespace Application.DeferredPayments.Queries.GetDeferredPaymentStatement;

public sealed class GetDeferredPaymentStatementQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetDeferredPaymentStatementQuery, DeferredPaymentStatementDto>
{
    public async Task<DeferredPaymentStatementDto> Handle(
        GetDeferredPaymentStatementQuery request,
        CancellationToken cancellationToken)
    {
        var payment = await context.DeferredPayment
            .AsNoTracking()
            .Include(d => d.Transactions)
            .FirstOrDefaultAsync(d => d.Id == request.DeferredPaymentId, cancellationToken)
            ?? throw new Exception("سجل الدفع الآجل غير موجود.");

        var invoice = await context.SalesInvoice
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == payment.SalesInvoiceId, cancellationToken)
            ?? throw new Exception("الفاتورة غير موجودة.");

        var customer = await context.Customer
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == payment.CustomerId, cancellationToken)
            ?? throw new Exception("العميل غير موجود.");

        return new DeferredPaymentStatementDto(
            payment.Id,
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.SaleDate,
            customer.Name,
            customer.Phone,
            payment.TotalAmount,
            payment.PaidAmount,
            payment.RemainingAmount,
            payment.IsFullyPaid,
            payment.Transactions
                .OrderByDescending(t => t.PaymentDate)
                .Select(t => new DeferredPaymentTransactionDto(
                    t.Id,
                    t.AmountPaid,
                    t.PaymentDate,
                    t.Notes))
                .ToList());
    }
}
