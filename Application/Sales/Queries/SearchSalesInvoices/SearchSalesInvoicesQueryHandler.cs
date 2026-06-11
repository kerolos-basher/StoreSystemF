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

        if (request.IsDeferredPayment.HasValue)
            query = query.Where(x => x.IsDeferredPayment == request.IsDeferredPayment.Value);

        if (!string.IsNullOrWhiteSpace(request.CustomerTerm))
        {
            var term = request.CustomerTerm.Trim().ToLower();
            var customerIds = await context.Customer
                .AsNoTracking()
                .Where(c => c.Name.ToLower().Contains(term) || c.Phone.Contains(request.CustomerTerm.Trim()))
                .Select(c => c.Id)
                .ToListAsync(cancellationToken);

            query = query.Where(x => x.CustomerId.HasValue && customerIds.Contains(x.CustomerId.Value));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var invoices = await query
            .OrderByDescending(x => x.SaleDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var customerIdList = invoices.Where(x => x.CustomerId.HasValue).Select(x => x.CustomerId!.Value).Distinct().ToList();
        var customers = await context.Customer
            .AsNoTracking()
            .Where(c => customerIdList.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, cancellationToken);

        var items = invoices.Select(x =>
        {
            customers.TryGetValue(x.CustomerId ?? 0, out var customer);
            return new SalesInvoiceListItemDto(
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
                    0,
                    i.UnitPrice,
                    i.LineTotal,
                    i.Notes)).ToList());
        }).ToList();

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
