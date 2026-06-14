namespace Application.Sales.Dtos;

public sealed record CreateSaleResultDto(
    long InvoiceId,
    string InvoiceNumber,
    decimal GrandTotal);

public sealed record SaleLineRequestDto(
    long ProductDetailsId,
    int Quantity,
    decimal UnitPrice,
    string Notes);

public sealed record SalesInvoiceDto(
    long Id,
    string InvoiceNumber,
    DateTime SaleDate,
    long? CustomerId,
    string? CustomerName,
    string? CustomerPhone,
    decimal Subtotal,
    decimal GrandTotal,
    string Notes,
    bool IsDeferredPayment,
    IReadOnlyList<SalesInvoiceItemDto> Items);

public sealed record SalesInvoiceItemDto(
    long Id,
    long ProductId,
    long ProductDetailsId,
    string ProductName,
    int Quantity,
    int ReturnedQuantity,
    int AvailableForReturn,
    int StockAvailable,
    decimal PurchasePrice,
    decimal UnitPrice,
    decimal LineTotal,
    string Notes);

public sealed record SalesInvoiceListItemDto(
    long Id,
    string InvoiceNumber,
    DateTime SaleDate,
    long? CustomerId,
    string? CustomerName,
    string? CustomerPhone,
    decimal Subtotal,
    decimal GrandTotal,
    bool IsDeferredPayment,
    int ItemCount,
    IReadOnlyList<SalesInvoiceItemDto> Items);

public sealed record UpdateSalesInvoiceItemDto(
    long? Id,
    long ProductDetailsId,
    int Quantity,
    decimal UnitPrice,
    string Notes);
