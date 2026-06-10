using Application.Abstractions.Persistence;
using Domain.InventoryAggregate;

namespace Application.Sales.Commands.DeleteSalesInvoice;

public sealed class DeleteSalesInvoiceCommandHandler(
    IApplicationDbContext context,
    ISequenceService sequenceService)
    : ICommandHandler<DeleteSalesInvoiceCommand>
{
    public async Task Handle(DeleteSalesInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await context.SalesInvoice
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new Exception("الفاتورة غير موجودة.");

        foreach (var item in invoice.Items.Where(x => !x.IsDeleted))
        {
            var returnable = item.AvailableForReturn;
            if (returnable > 0)
            {
                var product = await context.Product
                    .Include(p => p.ProductDetails)
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId, cancellationToken)
                    ?? throw new Exception("المنتج غير موجود.");

                product.RestoreStock(item.ProductDetailsId, returnable);

                var transactionId = await sequenceService.GetNextValueAsync(
                    SequenceKeys.InventoryTransactionSequence,
                    cancellationToken);

                context.InventoryTransaction.Add(
                    InventoryTransaction.CreateSaleReversal(
                        transactionId,
                        item.ProductId,
                        item.ProductDetailsId,
                        invoice.Id,
                        returnable,
                        invoice.InvoiceNumber));
            }
        }

        invoice.SoftDelete();

        var deferred = await context.DeferredPayment
            .FirstOrDefaultAsync(d => d.SalesInvoiceId == invoice.Id, cancellationToken);
        deferred?.SoftDelete();

        await context.SaveChangesAsync();
    }
}
