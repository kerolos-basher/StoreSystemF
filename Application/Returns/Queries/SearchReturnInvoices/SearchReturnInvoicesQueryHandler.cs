using Application.Abstractions.Persistence;
using Application.Returns.Dtos;
using Utilities.Response;

namespace Application.Returns.Queries.SearchReturnInvoices;

public sealed class SearchReturnInvoicesQueryHandler(IApplicationDbContext context)
    : IQueryHandler<SearchReturnInvoicesQuery, PagedResponse<ReturnInvoiceListItemDto>>
{
    public async Task<PagedResponse<ReturnInvoiceListItemDto>> Handle(
        SearchReturnInvoicesQuery request,
        CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        var query = context.ReturnInvoice
            .AsNoTracking()
            .Include(x => x.Items)
            .AsQueryable();

        if (request.DateFrom.HasValue)
        {
            var from = request.DateFrom.Value.Date;
            query = query.Where(x => x.ReturnDate >= from);
        }

        if (request.DateTo.HasValue)
        {
            var to = request.DateTo.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(x => x.ReturnDate <= to);
        }

        if (!string.IsNullOrWhiteSpace(request.ReturnNumber))
        {
            var term = request.ReturnNumber.Trim().ToLower();
            query = query.Where(x => x.ReturnNumber.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var returnIds = await query
            .OrderByDescending(x => x.ReturnDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var returns = await context.ReturnInvoice
            .AsNoTracking()
            .Include(x => x.Items)
            .Where(x => returnIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        var salesInvoiceIds = returns.Select(x => x.SalesInvoiceId).Distinct().ToList();
        var salesInvoices = await context.SalesInvoice
            .AsNoTracking()
            .Where(x => salesInvoiceIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.InvoiceNumber, cancellationToken);

        var items = returns
            .OrderByDescending(x => x.ReturnDate)
            .Select(x => new ReturnInvoiceListItemDto(
                x.Id,
                x.ReturnNumber,
                x.SalesInvoiceId,
                salesInvoices.GetValueOrDefault(x.SalesInvoiceId, string.Empty),
                x.ReturnDate,
                x.TotalAmount,
                x.Items.Count))
            .ToList();

        return new PagedResponse<ReturnInvoiceListItemDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }
}
