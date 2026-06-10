using Application.Sales.Dtos;

namespace Application.Sales.Commands.CreateSale;

public sealed record CreateSaleCommand(
    IReadOnlyList<SaleLineRequestDto> Items,
    string? CustomerName,
    string? CustomerPhone,
    long? CustomerId,
    string Notes,
    bool IsDeferredPayment,
    decimal AmountPaid) : ICommand<CreateSaleResultDto>;
