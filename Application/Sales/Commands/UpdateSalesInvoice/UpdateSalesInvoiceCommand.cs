using Application.Sales.Dtos;

namespace Application.Sales.Commands.UpdateSalesInvoice;

public sealed record UpdateSalesInvoiceCommand(
    long Id,
    string? Notes,
    bool IsDeferredPayment,
    IReadOnlyList<UpdateSalesInvoiceItemDto> Items) : ICommand;
