namespace Application.Sales.Dtos;

public sealed record CreateSaleResultDto(
    long InvoiceId,
    string InvoiceNumber,
    decimal GrandTotal);

public sealed record SaleLineRequestDto(
    long ProductId,
    long? ProductDetailsId,
    int Quantity,
    string Notes);

public sealed record SalesInvoiceDto(
    long Id,
    string InvoiceNumber,
    DateTime SaleDate,
    long? CustomerId,
    decimal Subtotal,
    decimal Discount,
    decimal Tax,
    decimal GrandTotal,
    string Notes,
    IReadOnlyList<SalesInvoiceItemDto> Items);

public sealed record SalesInvoiceItemDto(
    long Id,
    long ProductId,
    long ProductDetailsId,
    string ProductName,
    int Quantity,
    int ReturnedQuantity,
    int AvailableForReturn,
    decimal UnitPrice,
    decimal LineTotal,
    string Notes);

public sealed record SalesInvoiceListItemDto(
    long Id,
    string InvoiceNumber,
    DateTime SaleDate,
    decimal Subtotal,
    decimal Discount,
    decimal Tax,
    decimal GrandTotal,
    int ItemCount,
    IReadOnlyList<SalesInvoiceItemDto> Items);
