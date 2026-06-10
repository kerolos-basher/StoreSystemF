using Application.Abstractions.Persistence;
using Application.Sales.Dtos;

namespace Application.Sales.Queries.SearchSalesInvoicesByCustomer;

public sealed class SearchSalesInvoicesByCustomerQueryHandler(IApplicationDbContext context)
    : IQueryHandler<SearchSalesInvoicesByCustomerQuery, IReadOnlyList<SalesInvoiceListItemDto>>
{
    public async Task<IReadOnlyList<SalesInvoiceListItemDto>> Handle(
        SearchSalesInvoicesByCustomerQuery request,
        CancellationToken cancellationToken)
    {
        var customer = await context.Customer
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);

        var invoices = await context.SalesInvoice
            .AsNoTracking()
            .Include(x => x.Items)
            .Where(x => x.CustomerId == request.CustomerId)
            .OrderByDescending(x => x.SaleDate)
            .ToListAsync(cancellationToken);

        return invoices.Select(x => new SalesInvoiceListItemDto(
            x.Id,
            x.InvoiceNumber,
            x.SaleDate,
            x.CustomerId,
            customer?.Name,
            customer?.Phone,
            x.Subtotal,
            x.GrandTotal,
            x.IsDeferredPayment,
            x.Items.Count(i => !i.IsDeleted),
            x.Items.Where(i => !i.IsDeleted).Select(i => new SalesInvoiceItemDto(
                i.Id,
                i.ProductId,
                i.ProductDetailsId,
                i.ProductName,
                i.Quantity,
                i.ReturnedQuantity,
                i.AvailableForReturn,
                i.UnitPrice,
                i.LineTotal,
                i.Notes)).ToList())).ToList();
    }
}
