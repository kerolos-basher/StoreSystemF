using Application.Abstractions.Persistence;

namespace Application.DeferredPayments.Commands.DeleteDeferredPaymentTransaction;

public sealed class DeleteDeferredPaymentTransactionCommandHandler(IApplicationDbContext context)
    : ICommandHandler<DeleteDeferredPaymentTransactionCommand>
{
    public async Task Handle(DeleteDeferredPaymentTransactionCommand request, CancellationToken cancellationToken)
    {
        var payment = await context.DeferredPayment
            .Include(d => d.Transactions)
            .FirstOrDefaultAsync(d => d.Id == request.DeferredPaymentId, cancellationToken)
            ?? throw new StoreException("سجل الدفع الآجل غير موجود.");

        payment.DeleteTransaction(request.TransactionId);
        await context.SaveChangesAsync();
    }
}
