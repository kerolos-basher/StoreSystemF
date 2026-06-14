using Application.Abstractions.Persistence;
using Domain.InventoryAggregate;
using Domain.SalesAggregate;

namespace Application.Sales.Commands.UpdateSalesInvoice;

public sealed class UpdateSalesInvoiceCommandHandler(
    IApplicationDbContext context,
    ISequenceService sequenceService)
    : ICommandHandler<UpdateSalesInvoiceCommand>
{
    public async Task Handle(UpdateSalesInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await context.SalesInvoice
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new StoreException("الفاتورة غير موجودة.");

        invoice.UpdateNotes(request.Notes ?? string.Empty, request.IsDeferredPayment);

        var existingItems = invoice.Items.Where(x => !x.IsDeleted).ToList();
        var requestIds = request.Items.Where(x => x.Id.HasValue).Select(x => x.Id!.Value).ToHashSet();

        foreach (var existing in existingItems.Where(x => !requestIds.Contains(x.Id)))
        {
            await RestoreStockAsync(invoice, existing, existing.AvailableForReturn, cancellationToken);
            existing.SoftDelete();
        }

        foreach (var line in request.Items)
        {
            if (line.Id.HasValue)
            {
                var item = existingItems.FirstOrDefault(x => x.Id == line.Id.Value)
                    ?? throw new StoreException("بند الفاتورة غير موجود.");

                var diff = line.Quantity - item.Quantity;
                if (diff > 0)
                    await DeductStockAsync(item.ProductDetailsId, diff, cancellationToken);
                else if (diff < 0)
                    await RestoreStockAsync(invoice, item, Math.Abs(diff), cancellationToken);

                item.UpdateQuantity(line.Quantity);
                item.UpdateUnitPrice(line.UnitPrice);
            }
            else
            {
                var details = await context.ProductDetails
                    .Include(pd => pd.Product)
                    .FirstOrDefaultAsync(pd => pd.Id == line.ProductDetailsId, cancellationToken)
                    ?? throw new StoreException("تفاصيل المنتج غير موجودة.");

                if (details.Product.IsDeleted)
                    throw new StoreException("لا يمكن بيع منتج محذوف.");

                details.DeductStock(line.Quantity);

                var itemId = await sequenceService.GetNextValueAsync(SequenceKeys.SalesInvoiceItemSequence, cancellationToken);
                invoice.AddItem(
                    itemId,
                    details.ProductId,
                    details.Id,
                    details.Product.ProductName,
                    line.Quantity,
                    line.UnitPrice,
                    line.Notes ?? string.Empty);

                var transactionId = await sequenceService.GetNextValueAsync(SequenceKeys.InventoryTransactionSequence, cancellationToken);
                context.InventoryTransaction.Add(
                    InventoryTransaction.CreateSale(
                        transactionId,
                        details.ProductId,
                        details.Id,
                        invoice.Id,
                        line.Quantity,
                        invoice.InvoiceNumber));
            }
        }

        invoice.FinalizeInvoice();

        var deferredPayment = await context.DeferredPayment
            .FirstOrDefaultAsync(d => d.SalesInvoiceId == invoice.Id && !d.IsDeleted, cancellationToken);

        if (deferredPayment is not null)
            deferredPayment.SyncInvoiceTotal(invoice.GrandTotal);

        await context.SaveChangesAsync();
    }

    private async Task DeductStockAsync(long productDetailsId, int quantity, CancellationToken ct)
    {
        var details = await context.ProductDetails
            .FirstOrDefaultAsync(pd => pd.Id == productDetailsId, ct)
            ?? throw new StoreException("تفاصيل المنتج غير موجودة.");

        details.DeductStock(quantity);
    }

    private async Task RestoreStockAsync(
        SalesInvoice invoice,
        SalesInvoiceItem item,
        int quantity,
        CancellationToken ct)
    {
        if (quantity <= 0) return;

        var product = await context.Product
            .Include(p => p.ProductDetails)
            .FirstOrDefaultAsync(p => p.Id == item.ProductId, ct)
            ?? throw new StoreException("المنتج غير موجود.");

        product.RestoreStock(item.ProductDetailsId, quantity);

        var transactionId = await sequenceService.GetNextValueAsync(SequenceKeys.InventoryTransactionSequence, ct);
        context.InventoryTransaction.Add(
            InventoryTransaction.CreateSaleReversal(
                transactionId,
                item.ProductId,
                item.ProductDetailsId,
                invoice.Id,
                quantity,
                invoice.InvoiceNumber));
    }
}
