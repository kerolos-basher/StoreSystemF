using Application.Abstractions.Persistence;
using Application.Sales.Dtos;

namespace Application.Sales.Queries.GetSalesInvoice;

public sealed class GetSalesInvoiceQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetSalesInvoiceQuery, SalesInvoiceDto>
{
    public async Task<SalesInvoiceDto> Handle(
        GetSalesInvoiceQuery request,
        CancellationToken cancellationToken)
    {
        var invoice = await context.SalesInvoice
            .AsNoTracking()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == request.InvoiceId, cancellationToken)
            ?? throw new Exception("الفاتورة غير موجودة.");

        string? customerName = null;
        string? customerPhone = null;
        if (invoice.CustomerId.HasValue)
        {
            var customer = await context.Customer
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == invoice.CustomerId.Value, cancellationToken);
            customerName = customer?.Name;
            customerPhone = customer?.Phone;
        }

        return Map(invoice, customerName, customerPhone);
    }

    internal static SalesInvoiceDto Map(
        Domain.SalesAggregate.SalesInvoice invoice,
        string? customerName,
        string? customerPhone) =>
        new(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.SaleDate,
            invoice.CustomerId,
            customerName,
            customerPhone,
            invoice.Subtotal,
            invoice.GrandTotal,
            invoice.Notes,
            invoice.IsDeferredPayment,
            invoice.Items.Where(x => !x.IsDeleted).Select(x => new SalesInvoiceItemDto(
                x.Id,
                x.ProductId,
                x.ProductDetailsId,
                x.ProductName,
                x.Quantity,
                x.ReturnedQuantity,
                x.AvailableForReturn,
                x.UnitPrice,
                x.LineTotal,
                x.Notes)).ToList());
}
