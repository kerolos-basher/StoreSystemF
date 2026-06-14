using Application.Abstractions.Persistence;
using Domain.SalesAggregate;

namespace Application.Reports.Queries.GetFinancialReport;

public sealed class GetFinancialReportQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetFinancialReportQuery, FinancialReportDto>
{
    public async Task<FinancialReportDto> Handle(
        GetFinancialReportQuery request,
        CancellationToken cancellationToken)
    {
        var from = request.From?.Date ?? DateTime.Now.Date.AddMonths(-1);
        var to = request.To?.Date.AddDays(1).AddTicks(-1) ?? DateTime.Now;

        var invoices = await context.SalesInvoice
            .AsNoTracking()
            .Include(x => x.Items)
            .Where(x => !x.IsDeleted && x.SaleDate >= from && x.SaleDate <= to)
            .ToListAsync(cancellationToken);

        var netSales = invoices.Sum(GetInvoiceNetTotal);
        var cashSales = invoices.Where(x => !x.IsDeferredPayment).Sum(GetInvoiceNetTotal);
        var deferredSales = invoices.Where(x => x.IsDeferredPayment).Sum(GetInvoiceNetTotal);

        var detailsIds = invoices
            .SelectMany(x => x.Items.Where(i => !i.IsDeleted))
            .Select(x => x.ProductDetailsId)
            .Distinct()
            .ToList();

        var detailsPrices = await context.ProductDetails
            .AsNoTracking()
            .Where(pd => detailsIds.Contains(pd.Id))
            .ToDictionaryAsync(pd => pd.Id, pd => pd.Price, cancellationToken);

        decimal totalCost = 0;
        foreach (var invoice in invoices)
        {
            foreach (var item in invoice.Items.Where(i => !i.IsDeleted))
            {
                if (detailsPrices.TryGetValue(item.ProductDetailsId, out var cost))
                {
                    var netQty = item.Quantity - item.ReturnedQuantity;
                    if (netQty > 0)
                        totalCost += cost * netQty;
                }
            }
        }

        var outstanding = await context.DeferredPayment
            .AsNoTracking()
            .Where(d => !d.IsDeleted)
            .SumAsync(d => d.RemainingAmount, cancellationToken);

        var inventoryValue = await context.ProductDetails
            .AsNoTracking()
            .Where(pd => !pd.IsDeleted)
            .SumAsync(pd => pd.Price * pd.RemainingQuantity, cancellationToken);

        var topProducts = invoices
            .SelectMany(x => x.Items.Where(i => !i.IsDeleted))
            .GroupBy(x => x.ProductName)
            .Select(g => new TopProductDto(
                g.Key,
                g.Sum(x => x.Quantity - x.ReturnedQuantity),
                g.Sum(x => (x.Quantity - x.ReturnedQuantity) * x.UnitPrice)))
            .Where(x => x.QuantitySold > 0)
            .OrderByDescending(x => x.QuantitySold)
            .Take(5)
            .ToList();

        var customerIds = invoices.Where(x => x.CustomerId.HasValue).Select(x => x.CustomerId!.Value).Distinct().ToList();
        var customers = await context.Customer
            .AsNoTracking()
            .Where(c => customerIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        var topCustomers = invoices
            .Where(x => x.CustomerId.HasValue)
            .GroupBy(x => x.CustomerId!.Value)
            .Select(g => new TopCustomerDto(
                customers.GetValueOrDefault(g.Key, "—"),
                g.Count(),
                g.Sum(GetInvoiceNetTotal)))
            .OrderByDescending(x => x.TotalSpent)
            .Take(5)
            .ToList();

        return new FinancialReportDto(
            netSales,
            totalCost,
            netSales - totalCost,
            cashSales,
            deferredSales,
            outstanding,
            inventoryValue,
            topProducts,
            topCustomers);
    }

    private static decimal GetInvoiceNetTotal(SalesInvoice invoice) =>
        invoice.Items
            .Where(i => !i.IsDeleted)
            .Sum(i => (i.Quantity - i.ReturnedQuantity) * i.UnitPrice);
}
