using Application.Abstractions.Persistence;
using Application.Sales.Dtos;
using Domain.InventoryAggregate;
using Domain.ProductAggregate;
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


        var invoiceNumber = Guid.NewGuid().ToString();
        var invoice = SalesInvoice.Create(
            invoiceNumber,
            request.Discount,
            request.Tax,
            request.Notes ?? string.Empty);

        var transactionDrafts = new List<(long ProductId, long ProductDetailsId, int Quantity)>();

        foreach (var line in request.Items)
        {
            var product = await context.Product
                .Include(p => p.ProductDetails)
                .FirstOrDefaultAsync(p => p.Id == line.ProductId);
            if (product == null)
                throw new Exception("المنتج غير موجود.");

            if (product.IsDeleted)
                throw new Exception("لا يمكن بيع منتج محذوف.");

            var allocations = product.ReduceStockFifo(line.Quantity);

            foreach (var allocation in allocations)
            {
                invoice.AddItem(
                    product.Id,
                    allocation.ProductDetailsId,
                    product.ProductName,
                    allocation.Quantity,
                    allocation.UnitPrice,
                    line.Notes ?? string.Empty);

                transactionDrafts.Add((product.Id, allocation.ProductDetailsId, allocation.Quantity));
            }
        }

        invoice.FinalizeInvoice();
        context.SalesInvoice.Add(invoice);
        await context.SaveChangesAsync();

        foreach (var draft in transactionDrafts)
        {
            context.InventoryTransaction.Add(
                InventoryTransaction.CreateSale(
                    draft.ProductId,
                    draft.ProductDetailsId,
                    invoice.Id,
                    draft.Quantity,
                    invoiceNumber));
        }

        await context.SaveChangesAsync();

        return new CreateSaleResultDto(
            invoice.Id.ToString(),
            invoice.InvoiceNumber,
            invoice.GrandTotal);
    }
}
