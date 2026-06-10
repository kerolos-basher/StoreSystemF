using Application.Abstractions.Persistence;
using Application.Sales.Dtos;
using Domain.CustomerAggregate;
using Domain.DeferredPaymentAggregate;
using Domain.InventoryAggregate;
using Domain.SalesAggregate;

namespace Application.Sales.Commands.CreateSale;

public sealed class CreateSaleCommandHandler(
    IApplicationDbContext context,
    ISequenceService sequenceService)
    : ICommandHandler<CreateSaleCommand, CreateSaleResultDto>
{
    public async Task<CreateSaleResultDto> Handle(
        CreateSaleCommand request,
        CancellationToken cancellationToken)
    {
        long? customerId = request.CustomerId;

        if (!string.IsNullOrWhiteSpace(request.CustomerName))
        {
            if (customerId.HasValue)
            {
                var existing = await context.Customer
                    .FirstOrDefaultAsync(c => c.Id == customerId.Value, cancellationToken);
                if (existing is not null)
                    existing.Update(request.CustomerName, request.CustomerPhone);
            }
            else
            {
                var newCustomerId = await sequenceService.GetNextValueAsync(SequenceKeys.CustomerSequence, cancellationToken);
                var customer = Customer.Create(newCustomerId, request.CustomerName, request.CustomerPhone);
                context.Customer.Add(customer);
                await context.SaveChangesAsync();
                customerId = customer.Id;
            }
        }

        var invoiceId = await sequenceService.GetNextValueAsync(SequenceKeys.SalesInvoiceSequence, cancellationToken);
        var invoice = SalesInvoice.Create(
            invoiceId,
            request.Notes ?? string.Empty,
            customerId,
            request.IsDeferredPayment);

        var transactionDrafts = new List<(long TransactionId, long ProductId, long ProductDetailsId, int Quantity)>();

        foreach (var line in request.Items)
        {
            var details = await context.ProductDetails
                .Include(pd => pd.Product)
                .FirstOrDefaultAsync(pd => pd.Id == line.ProductDetailsId, cancellationToken)
                ?? throw new Exception("تفاصيل المنتج غير موجودة.");

            if (details.Product.IsDeleted)
                throw new Exception("لا يمكن بيع منتج محذوف.");

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
            transactionDrafts.Add((transactionId, details.ProductId, details.Id, line.Quantity));
        }

        invoice.FinalizeInvoice();
        context.SalesInvoice.Add(invoice);

        if (request.IsDeferredPayment)
        {
            if (!customerId.HasValue)
                throw new Exception("يجب تحديد عميل للدفع الآجل.");

            var deferredPaymentId = await sequenceService.GetNextValueAsync(SequenceKeys.DeferredPaymentSequence, cancellationToken);
            context.DeferredPayment.Add(DeferredPayment.Create(
                deferredPaymentId,
                invoice.Id,
                customerId.Value,
                invoice.GrandTotal,
                request.Notes));
        }

        foreach (var draft in transactionDrafts)
        {
            context.InventoryTransaction.Add(
                InventoryTransaction.CreateSale(
                    draft.TransactionId,
                    draft.ProductId,
                    draft.ProductDetailsId,
                    invoice.Id,
                    draft.Quantity,
                    invoice.InvoiceNumber));
        }

        await context.SaveChangesAsync();

        return new CreateSaleResultDto(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.GrandTotal);
    }
}
