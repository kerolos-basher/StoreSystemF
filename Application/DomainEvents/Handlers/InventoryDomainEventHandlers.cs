using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.DomainEvents.Handlers;

public sealed class InventoryAddedDomainEventHandler(ILogger<InventoryAddedDomainEventHandler> logger)
    : INotificationHandler<InventoryAddedDomainEvent>
{
    public Task Handle(InventoryAddedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Inventory added: ProductDetails {Id}, Qty {Qty}", notification.ProductDetailsId, notification.Quantity);
        return Task.CompletedTask;
    }
}

public sealed class StockDeductedDomainEventHandler(ILogger<StockDeductedDomainEventHandler> logger)
    : INotificationHandler<StockDeductedDomainEvent>
{
    public Task Handle(StockDeductedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Stock deducted: ProductDetails {Id}, Qty {Qty}", notification.ProductDetailsId, notification.Quantity);
        return Task.CompletedTask;
    }
}

public sealed class ItemReturnedDomainEventHandler(ILogger<ItemReturnedDomainEventHandler> logger)
    : INotificationHandler<ItemReturnedDomainEvent>
{
    public Task Handle(ItemReturnedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Item returned: SalesInvoiceItem {Id}, Qty {Qty}", notification.SalesInvoiceItemId, notification.Quantity);
        return Task.CompletedTask;
    }
}

public sealed class InvoiceDeletedDomainEventHandler(ILogger<InvoiceDeletedDomainEventHandler> logger)
    : INotificationHandler<InvoiceDeletedDomainEvent>
{
    public Task Handle(InvoiceDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Invoice deleted: {Id}", notification.SalesInvoiceId);
        return Task.CompletedTask;
    }
}

public sealed class ProductDeletedDomainEventHandler(ILogger<ProductDeletedDomainEventHandler> logger)
    : INotificationHandler<ProductDeletedDomainEvent>
{
    public Task Handle(ProductDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Product deleted: {Id}", notification.ProductId);
        return Task.CompletedTask;
    }
}

public sealed class InventoryRemovedDomainEventHandler(ILogger<InventoryRemovedDomainEventHandler> logger)
    : INotificationHandler<InventoryRemovedDomainEvent>
{
    public Task Handle(InventoryRemovedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Inventory removed: ProductDetails {Id}, Qty {Qty}", notification.ProductDetailsId, notification.Quantity);
        return Task.CompletedTask;
    }
}
