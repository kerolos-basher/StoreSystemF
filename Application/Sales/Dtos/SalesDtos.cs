namespace Application.Sales.Dtos;

public sealed record CreateSaleResultDto(
    string InvoiceId,
    string InvoiceNumber,
    decimal GrandTotal);

public sealed record SaleLineRequestDto(
    long ProductId,
    int Quantity,
    string Notes);

public sealed record SalesInvoiceDto(
    string Id,
    string InvoiceNumber,
    DateTime SaleDate,
    decimal Subtotal,
    decimal Discount,
    decimal Tax,
    decimal GrandTotal,
    string Notes,
    IReadOnlyList<SalesInvoiceItemDto> Items);

public sealed record SalesInvoiceItemDto(
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    string Notes);

public sealed record SalesInvoiceListItemDto(
    string Id,
    string InvoiceNumber,
    DateTime SaleDate,
    decimal Subtotal,
    decimal Discount,
    decimal Tax,
    decimal GrandTotal,
    int ItemCount,
    IReadOnlyList<SalesInvoiceItemDto> Items);
