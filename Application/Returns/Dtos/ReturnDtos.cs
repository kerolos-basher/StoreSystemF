namespace Application.Returns.Dtos;

public sealed record CreateReturnResultDto(
    long ReturnInvoiceId,
    string ReturnNumber,
    decimal TotalAmount);

public sealed record ReturnLineRequestDto(
    long SalesInvoiceItemId,
    int Quantity,
    decimal UnitPrice,
    int ItemReasonType,
    string Notes);

public sealed record ReturnInvoiceItemDto(
    long Id,
    long SalesInvoiceItemId,
    long ProductId,
    long ProductDetailsId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    int ItemReasonType,
    bool IsReturnToStock,
    string Notes);

public sealed record ReturnInvoiceDto(
    long Id,
    string ReturnNumber,
    long SalesInvoiceId,
    string SalesInvoiceNumber,
    DateTime ReturnDate,
    decimal TotalAmount,
    int ReturnReasonType,
    string Notes,
    IReadOnlyList<ReturnInvoiceItemDto> Items);

public sealed record ReturnInvoiceListItemDto(
    long Id,
    string ReturnNumber,
    long SalesInvoiceId,
    string SalesInvoiceNumber,
    DateTime ReturnDate,
    decimal TotalAmount,
    int ItemCount);
