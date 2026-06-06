using Application.Abstractions.Persistence;
using Application.Returns.Dtos;

namespace Application.Returns.Queries.GetReturnInvoice;

public sealed class GetReturnInvoiceQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetReturnInvoiceQuery, ReturnInvoiceDto>
{
    public async Task<ReturnInvoiceDto> Handle(
        GetReturnInvoiceQuery request,
        CancellationToken cancellationToken)
    {
        var returnInvoice = await context.ReturnInvoice
            .AsNoTracking()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == request.ReturnInvoiceId, cancellationToken)
            ?? throw new Exception("مرتجع الفاتورة غير موجود.");

        var salesInvoice = await context.SalesInvoice
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == returnInvoice.SalesInvoiceId, cancellationToken);

        var productNames = await context.SalesInvoiceItem
            .AsNoTracking()
            .Where(x => returnInvoice.Items.Select(i => i.SalesInvoiceItemId).Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.ProductName, cancellationToken);

        return new ReturnInvoiceDto(
            returnInvoice.Id,
            returnInvoice.ReturnNumber,
            returnInvoice.SalesInvoiceId,
            salesInvoice?.InvoiceNumber ?? string.Empty,
            returnInvoice.ReturnDate,
            returnInvoice.TotalAmount,
            returnInvoice.ReturnReasonType,
            returnInvoice.Notes,
            returnInvoice.Items.Select(x => new ReturnInvoiceItemDto(
                x.Id,
                x.SalesInvoiceItemId,
                x.ProductId,
                x.ProductDetailsId,
                productNames.GetValueOrDefault(x.SalesInvoiceItemId, string.Empty),
                x.Quantity,
                x.UnitPrice,
                x.LineTotal,
                x.ItemReasonType,
                x.IsReturnToStock,
                x.Notes)).ToList());
    }
}
