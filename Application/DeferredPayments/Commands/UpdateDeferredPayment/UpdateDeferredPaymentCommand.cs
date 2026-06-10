namespace Application.DeferredPayments.Commands.UpdateDeferredPayment;

public sealed record UpdateDeferredPaymentCommand(
    long DeferredPaymentId,
    decimal AmountPaid,
    string? Notes) : ICommand;
