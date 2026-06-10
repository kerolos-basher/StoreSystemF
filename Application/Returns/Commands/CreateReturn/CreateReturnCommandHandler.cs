using Application.Abstractions.Persistence;
using Application.Returns.Dtos;
using Domain.InventoryAggregate;
using Domain.ReturnsAggregate;

namespace Application.Returns.Commands.CreateReturn;

public sealed class CreateReturnCommandHandler(
    IApplicationDbContext context,
    ISequenceService sequenceService)
    : ICommandHandler<CreateReturnCommand, CreateReturnResultDto>
{
    public async Task<CreateReturnResultDto> Handle(
        CreateReturnCommand request,
        CancellationToken cancellationToken)
    {
        var invoice = await context.SalesInvoice
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == request.SalesInvoiceId, cancellationToken)
            ?? throw new Exception("الفاتورة غير موجودة.");

        var returnInvoiceId = await sequenceService.GetNextValueAsync(SequenceKeys.ReturnInvoiceSequence, cancellationToken);
        var returnInvoice = ReturnInvoice.Create(
            returnInvoiceId,
            request.SalesInvoiceId,
            request.ReturnReasonType,
            request.Notes ?? string.Empty);

        foreach (var line in request.Items)
        {
            var invoiceItem = invoice.Items.FirstOrDefault(x => x.Id == line.SalesInvoiceItemId)
                ?? throw new Exception("بند الفاتورة غير موجود.");

            if (line.Quantity > invoiceItem.AvailableForReturn)
                throw new Exception($"الكمية المرتجعة تتجاوز المتاح للصنف {invoiceItem.ProductName}.");

            var reason = await context.ReturnReason
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == line.ItemReasonType, cancellationToken)
                ?? throw new Exception("سبب المرتجع غير موجود.");

            invoiceItem.RegisterReturn(line.Quantity);

            var returnItemId = await sequenceService.GetNextValueAsync(SequenceKeys.ReturnInvoiceItemSequence, cancellationToken);
            returnInvoice.AddItem(
                returnItemId,
                invoiceItem.Id,
                invoiceItem.ProductId,
                invoiceItem.ProductDetailsId,
                line.Quantity,
                invoiceItem.UnitPrice,
                line.ItemReasonType,
                reason.IsReturnToStock,
                line.Notes ?? string.Empty);
        }

        returnInvoice.FinalizeReturn();
        context.ReturnInvoice.Add(returnInvoice);

        foreach (var item in returnInvoice.Items)
        {
            var transactionId = await sequenceService.GetNextValueAsync(SequenceKeys.InventoryTransactionSequence, cancellationToken);

            if (item.IsReturnToStock)
            {
                var product = await context.Product
                    .Include(p => p.ProductDetails)
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId, cancellationToken)
                    ?? throw new Exception("المنتج غير موجود.");

                product.RestoreStock(item.ProductDetailsId, item.Quantity);

                context.InventoryTransaction.Add(
                    InventoryTransaction.CreateCustomerReturn(
                        transactionId,
                        item.ProductId,
                        item.ProductDetailsId,
                        returnInvoice.Id,
                        item.Quantity,
                        returnInvoice.ReturnNumber));
            }
            else
            {
                context.InventoryTransaction.Add(
                    InventoryTransaction.CreateDamagedReturn(
                        transactionId,
                        item.ProductId,
                        item.ProductDetailsId,
                        returnInvoice.Id,
                        returnInvoice.ReturnNumber));
            }
        }

        await context.SaveChangesAsync();

        return new CreateReturnResultDto(
            returnInvoice.Id,
            returnInvoice.ReturnNumber,
            returnInvoice.TotalAmount);
    }
}
