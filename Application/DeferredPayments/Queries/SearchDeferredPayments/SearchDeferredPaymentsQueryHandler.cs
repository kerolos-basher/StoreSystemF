using Application.Abstractions.Persistence;
using Utilities.Response;

namespace Application.DeferredPayments.Queries.SearchDeferredPayments;

public sealed class SearchDeferredPaymentsQueryHandler(IApplicationDbContext context)
    : IQueryHandler<SearchDeferredPaymentsQuery, PagedResponse<DeferredPaymentListItemDto>>
{
    public async Task<PagedResponse<DeferredPaymentListItemDto>> Handle(
        SearchDeferredPaymentsQuery request,
        CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize;

        var query = from dp in context.DeferredPayment.AsNoTracking()
                    join si in context.SalesInvoice.AsNoTracking() on dp.SalesInvoiceId equals si.Id
                    join c in context.Customer.AsNoTracking() on dp.CustomerId equals c.Id
                    select new { dp, si, c };

        if (!string.IsNullOrWhiteSpace(request.CustomerTerm))
        {
            var term = request.CustomerTerm.Trim().ToLower();
            query = query.Where(x =>
                x.c.Name.ToLower().Contains(term) ||
                x.c.Phone.Contains(request.CustomerTerm.Trim()));
        }

        if (request.IsFullyPaid.HasValue)
            query = query.Where(x => x.dp.IsFullyPaid == request.IsFullyPaid.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.si.SaleDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new DeferredPaymentListItemDto(
                x.dp.Id,
                x.si.Id,
                x.si.InvoiceNumber,
                x.si.SaleDate,
                x.c.Id,
                x.c.Name,
                x.c.Phone,
                x.dp.TotalAmount,
                x.dp.PaidAmount,
                x.dp.RemainingAmount,
                x.dp.IsFullyPaid))
            .ToListAsync(cancellationToken);

        return new PagedResponse<DeferredPaymentListItemDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }
}
