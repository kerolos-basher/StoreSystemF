using Application.Abstractions.Persistence;

namespace Application.DeferredPayments.Commands.UpdateDeferredPaymentTransaction;

public sealed class UpdateDeferredPaymentTransactionCommandHandler(IApplicationDbContext context)
    : ICommandHandler<UpdateDeferredPaymentTransactionCommand>
{
    public async Task Handle(UpdateDeferredPaymentTransactionCommand request, CancellationToken cancellationToken)
    {
        var payment = await context.DeferredPayment
            .Include(d => d.Transactions)
            .FirstOrDefaultAsync(d => d.Id == request.DeferredPaymentId, cancellationToken)
            ?? throw new StoreException("سجل الدفع الآجل غير موجود.");

        payment.UpdateTransaction(request.TransactionId, request.AmountPaid, request.Notes);
        await context.SaveChangesAsync();
    }
}
