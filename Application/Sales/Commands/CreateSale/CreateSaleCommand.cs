using Application.Sales.Dtos;

namespace Application.Sales.Commands.CreateSale;

public sealed record CreateSaleCommand(
    IReadOnlyList<SaleLineRequestDto> Items,
    decimal Discount,
    decimal Tax,
    string Notes) : ICommand<CreateSaleResultDto>;
