namespace Domain.Events;

public sealed record InventoryAddedDomainEvent(long ProductDetailsId, int Quantity) : IDomainEvent;

public sealed record StockDeductedDomainEvent(long ProductDetailsId, int Quantity) : IDomainEvent;

public sealed record ItemReturnedDomainEvent(long SalesInvoiceItemId, int Quantity) : IDomainEvent;

public sealed record InvoiceDeletedDomainEvent(long SalesInvoiceId) : IDomainEvent;

public sealed record ProductDeletedDomainEvent(long ProductId) : IDomainEvent;

public sealed record InventoryRemovedDomainEvent(long ProductDetailsId, int Quantity) : IDomainEvent;
