using Application.Abstractions.Persistence;

namespace Application.Reports.Queries.GetFinancialReport;

public sealed class GetFinancialReportQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetFinancialReportQuery, FinancialReportDto>
{
    public async Task<FinancialReportDto> Handle(
        GetFinancialReportQuery request,
        CancellationToken cancellationToken)
    {
        var from = request.From?.Date ?? DateTime.UtcNow.Date.AddMonths(-1);
        var to = request.To?.Date.AddDays(1).AddTicks(-1) ?? DateTime.UtcNow;

        var invoices = await context.SalesInvoice
            .AsNoTracking()
            .Include(x => x.Items)
            .Where(x => x.SaleDate >= from && x.SaleDate <= to)
            .ToListAsync(cancellationToken);

        var totalSales = invoices.Sum(x => x.GrandTotal);
        var cashSales = invoices.Where(x => !x.IsDeferredPayment).Sum(x => x.GrandTotal);
        var deferredSales = invoices.Where(x => x.IsDeferredPayment).Sum(x => x.GrandTotal);

        var itemIds = invoices.SelectMany(x => x.Items).Select(x => x.ProductDetailsId).Distinct().ToList();
        var detailsPrices = await context.ProductDetails
            .AsNoTracking()
            .Where(pd => itemIds.Contains(pd.Id))
            .ToDictionaryAsync(pd => pd.Id, pd => pd.Price, cancellationToken);

        decimal totalCost = 0;
        foreach (var invoice in invoices)
        {
            foreach (var item in invoice.Items)
            {
                if (detailsPrices.TryGetValue(item.ProductDetailsId, out var cost))
                    totalCost += cost * item.Quantity;
            }
        }

        var outstanding = await context.DeferredPayment
            .AsNoTracking()
            .SumAsync(d => d.RemainingAmount, cancellationToken);

        var inventoryValue = await context.ProductDetails
            .AsNoTracking()
            .SumAsync(pd => pd.Price * pd.RemainingQuantity, cancellationToken);

        var topProducts = invoices
            .SelectMany(x => x.Items)
            .GroupBy(x => x.ProductName)
            .Select(g => new TopProductDto(
                g.Key,
                g.Sum(x => x.Quantity),
                g.Sum(x => x.LineTotal)))
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
                g.Sum(x => x.GrandTotal)))
            .OrderByDescending(x => x.TotalSpent)
            .Take(5)
            .ToList();

        return new FinancialReportDto(
            totalSales,
            totalCost,
            totalSales - totalCost,
            cashSales,
            deferredSales,
            outstanding,
            inventoryValue,
            topProducts,
            topCustomers);
    }
}
