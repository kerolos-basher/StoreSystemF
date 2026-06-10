using Application.Abstractions.Persistence;
using Application.Abstractions.Services;
using Domain.InventoryAggregate;

namespace Infrastructure.Services.Inventory;

public sealed class InventoryTransactionService(
    IApplicationDbContext context,
    ISequenceService sequenceService) : IInventoryTransactionService
{
    public async Task RecordTransactionAsync(
        long productId,
        long productDetailsId,
        int quantityChange,
        InventoryTransactionType type,
        long? salesInvoiceId = null,
        long? returnInvoiceId = null,
        string? reference = null,
        CancellationToken cancellationToken = default)
    {
        var transactionId = await sequenceService.GetNextValueAsync(
            SequenceKeys.InventoryTransactionSequence,
            cancellationToken);

        var refText = reference ?? string.Empty;
        InventoryTransaction record = type switch
        {
            InventoryTransactionType.Purchase => InventoryTransaction.CreatePurchase(
                transactionId, productId, productDetailsId, quantityChange, refText),
            InventoryTransactionType.Sale => InventoryTransaction.CreateSale(
                transactionId, productId, productDetailsId, salesInvoiceId!.Value, Math.Abs(quantityChange), refText),
            InventoryTransactionType.CustomerReturn => InventoryTransaction.CreateCustomerReturn(
                transactionId, productId, productDetailsId, returnInvoiceId!.Value, quantityChange, refText),
            InventoryTransactionType.SaleReversal => InventoryTransaction.CreateSaleReversal(
                transactionId, productId, productDetailsId, salesInvoiceId!.Value, quantityChange, refText),
            _ => throw new NotSupportedException($"Transaction type {type} is not supported by the service.")
        };

        context.InventoryTransaction.Add(record);
        await context.SaveChangesAsync();
    }
}
