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

        return new SalesInvoiceDto(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.SaleDate,
            invoice.CustomerId,
            invoice.Subtotal,
            invoice.Discount,
            invoice.Tax,
            invoice.GrandTotal,
            invoice.Notes,
            invoice.Items.Select(x => new SalesInvoiceItemDto(
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
}
