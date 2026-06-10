using Application.Abstractions.Persistence;
using Application.Sales.Dtos;
using Application.Sales.Queries.GetSalesInvoice;

namespace Application.Sales.Queries.GetSalesInvoiceByNumber;

public sealed class GetSalesInvoiceByNumberQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetSalesInvoiceByNumberQuery, SalesInvoiceDto?>
{
    public async Task<SalesInvoiceDto?> Handle(
        GetSalesInvoiceByNumberQuery request,
        CancellationToken cancellationToken)
    {
        var number = request.InvoiceNumber?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(number))
            return null;

        var invoice = await context.SalesInvoice
            .AsNoTracking()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.InvoiceNumber == number, cancellationToken);

        if (invoice is null)
            return null;

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

        return GetSalesInvoiceQueryHandler.Map(invoice, customerName, customerPhone);
    }
}
