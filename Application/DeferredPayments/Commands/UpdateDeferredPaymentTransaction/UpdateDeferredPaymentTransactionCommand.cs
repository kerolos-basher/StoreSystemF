namespace Application.DeferredPayments.Commands.UpdateDeferredPaymentTransaction;

public sealed record UpdateDeferredPaymentTransactionCommand(
    long DeferredPaymentId,
    long TransactionId,
    decimal AmountPaid,
    string? Notes) : ICommand;
