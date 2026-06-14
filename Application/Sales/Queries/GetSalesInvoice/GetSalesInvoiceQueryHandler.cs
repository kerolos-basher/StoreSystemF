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
            ?? throw new StoreException("الفاتورة غير موجودة.");

        var detailsIds = invoice.Items
            .Where(x => !x.IsDeleted)
            .Select(x => x.ProductDetailsId)
            .Distinct()
            .ToList();

        var stockByDetails = await context.ProductDetails
            .AsNoTracking()
            .Where(pd => detailsIds.Contains(pd.Id))
            .ToDictionaryAsync(pd => pd.Id, pd => pd.RemainingQuantity, cancellationToken);

        var purchasePrices = await context.ProductDetails
            .AsNoTracking()
            .Where(pd => detailsIds.Contains(pd.Id))
            .ToDictionaryAsync(pd => pd.Id, pd => pd.Price, cancellationToken);

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

        return Map(invoice, customerName, customerPhone, stockByDetails, purchasePrices);
    }

    internal static SalesInvoiceDto Map(
        Domain.SalesAggregate.SalesInvoice invoice,
        string? customerName,
        string? customerPhone,
        IReadOnlyDictionary<long, int>? stockByDetails = null,
        IReadOnlyDictionary<long, decimal>? purchasePrices = null) =>
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
                stockByDetails?.GetValueOrDefault(x.ProductDetailsId, 0) ?? 0,
                purchasePrices?.GetValueOrDefault(x.ProductDetailsId, 0) ?? 0,
                x.UnitPrice,
                x.LineTotal,
                x.Notes)).ToList());
}
