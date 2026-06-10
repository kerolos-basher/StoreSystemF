using Domain.InventoryAggregate;

namespace Application.Abstractions.Services;

public interface IInventoryTransactionService
{
    Task RecordTransactionAsync(
        long productId,
        long productDetailsId,
        int quantityChange,
        InventoryTransactionType type,
        long? salesInvoiceId = null,
        long? returnInvoiceId = null,
        string? reference = null,
        CancellationToken cancellationToken = default);
}
