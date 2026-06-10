using Application.Abstractions.Persistence;

namespace Application.DeferredPayments.Commands.UpdateDeferredPayment;

public sealed class UpdateDeferredPaymentCommandHandler(
    IApplicationDbContext context,
    ISequenceService sequenceService)
    : ICommandHandler<UpdateDeferredPaymentCommand>
{
    public async Task Handle(UpdateDeferredPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await context.DeferredPayment
            .Include(d => d.Transactions)
            .FirstOrDefaultAsync(d => d.Id == request.DeferredPaymentId, cancellationToken)
            ?? throw new Exception("سجل الدفع الآجل غير موجود.");

        var transactionId = await sequenceService.GetNextValueAsync(
            SequenceKeys.DeferredPaymentTransactionSequence,
            cancellationToken);

        payment.RegisterPayment(transactionId, request.AmountPaid, request.Notes, 0);
        await context.SaveChangesAsync();
    }
}
