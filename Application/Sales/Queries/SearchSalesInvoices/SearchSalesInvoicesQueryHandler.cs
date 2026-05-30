using Application.Abstractions.Persistence;
using Application.Sales.Dtos;
using Utilities.Response;

namespace Application.Sales.Queries.SearchSalesInvoices;

public sealed class SearchSalesInvoicesQueryHandler(IApplicationDbContext context)
    : IQueryHandler<SearchSalesInvoicesQuery, PagedResponse<SalesInvoiceListItemDto>>
{
    public async Task<PagedResponse<SalesInvoiceListItemDto>> Handle(
        SearchSalesInvoicesQuery request,
        CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        var query = context.SalesInvoice
            .AsNoTracking()
            .Include(x => x.Items)
            .AsQueryable();

        if (request.DateFrom.HasValue)
        {
            var from = request.DateFrom.Value.Date;
            query = query.Where(x => x.SaleDate >= from);
        }

        if (request.DateTo.HasValue)
        {
            var to = request.DateTo.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(x => x.SaleDate <= to);
        }

        if (!string.IsNullOrWhiteSpace(request.InvoiceNumber))
        {
            var term = request.InvoiceNumber.Trim().ToLower();
            query = query.Where(x => x.InvoiceNumber.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.SaleDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new SalesInvoiceListItemDto(
                x.Id,
                x.InvoiceNumber,
                x.SaleDate,
                x.Subtotal,
                x.Discount,
                x.Tax,
                x.GrandTotal,
                x.Items.Count,
                x.Items.Select(i => new SalesInvoiceItemDto(
                    i.ProductName,
                    i.Quantity,
                    i.UnitPrice,
                    i.LineTotal,
                    i.Notes)).ToList()))
            .ToListAsync(cancellationToken);

        return new PagedResponse<SalesInvoiceListItemDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }
}
