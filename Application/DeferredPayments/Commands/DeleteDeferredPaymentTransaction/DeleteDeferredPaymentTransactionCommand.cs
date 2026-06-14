namespace Application.DeferredPayments.Commands.DeleteDeferredPaymentTransaction;

public sealed record DeleteDeferredPaymentTransactionCommand(
    long DeferredPaymentId,
    long TransactionId) : ICommand;
