using Application.Sales.Commands.CreateSale;
using Application.Sales.Commands.DeleteSalesInvoice;
using Application.Sales.Commands.UpdateSalesInvoice;
using Application.Sales.Dtos;
using Application.Sales.Queries.GetSalesInvoice;
using Application.Sales.Queries.GetSalesInvoiceByNumber;
using Application.Sales.Queries.SearchSalesInvoices;
using Infrastructure.Services.LogFile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Store_Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/sales")]
public sealed class SalesController : StoreBaseController
{
    private readonly ISender _sender;

    public SalesController(LogFileService logger, ISender sender) : base(logger)
    {
        _sender = sender;
    }

    [HttpPost]
    public Task<IActionResult> Create(
        [FromBody] CreateSaleRequest request,
        CancellationToken cancellationToken) =>
        TryCatchLogAsync(async () =>
        {
            var command = new CreateSaleCommand(
                request.Items ?? [],
                request.CustomerName,
                request.CustomerPhone,
                request.CustomerId,
                request.Notes ?? string.Empty,
                request.IsDeferredPayment,
                request.AmountPaid);

            var result = await _sender.Send(command, cancellationToken);
            return Ok(result);
        });

    [HttpGet("invoices")]
    public Task<IActionResult> SearchInvoices(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string invoiceNumber,
        [FromQuery] string customerTerm,
        [FromQuery] bool? isDeferredPayment,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default) =>
        TryCatchLogAsync(async () =>
        {
            var result = await _sender.Send(
                new SearchSalesInvoicesQuery(
                    dateFrom,
                    dateTo,
                    invoiceNumber ?? string.Empty,
                    customerTerm,
                    isDeferredPayment,
                    pageNumber,
                    pageSize),
                cancellationToken);

            return Ok(new
            {
                items = result.Items,
                totalCount = result.TotalCount,
                pageNumber = result.PageNumber,
                pageSize = result.PageSize
            });
        });

    [HttpGet("invoices/by-number/{number}")]
    public Task<IActionResult> GetByNumber(
        string number,
        CancellationToken cancellationToken) =>
        TryCatchLogAsync(async () =>
        {
            var invoice = await _sender.Send(new GetSalesInvoiceByNumberQuery(number), cancellationToken);
            if (invoice is null)
                return NotFound(new { message = "الفاتورة غير موجودة." });

            return Ok(invoice);
        });

    [HttpGet("{invoiceId}")]
    public Task<IActionResult> GetInvoice(
        string invoiceId,
        CancellationToken cancellationToken) =>
        TryCatchLogAsync(async () =>
        {
            if (!long.TryParse(invoiceId, out var id))
                return BadRequest(new { message = "معرف الفاتورة غير صالح." });

            var invoice = await _sender.Send(new GetSalesInvoiceQuery(id), cancellationToken);
            return Ok(invoice);
        });

    [HttpPut("{invoiceId}")]
    public Task<IActionResult> UpdateInvoice(
        long invoiceId,
        [FromBody] UpdateSalesInvoiceRequest request,
        CancellationToken cancellationToken) =>
        TryCatchLogAsync(async () =>
        {
            await _sender.Send(
                new UpdateSalesInvoiceCommand(
                    invoiceId,
                    request.Notes,
                    request.IsDeferredPayment,
                    request.Items ?? []),
                cancellationToken);

            return Ok();
        });

    [HttpDelete("{invoiceId}")]
    public Task<IActionResult> DeleteInvoice(
        long invoiceId,
        CancellationToken cancellationToken) =>
        TryCatchLogAsync(async () =>
        {
            await _sender.Send(new DeleteSalesInvoiceCommand(invoiceId), cancellationToken);
            return Ok();
        });

    public sealed class CreateSaleRequest
    {
        public List<SaleLineRequestDto> Items { get; set; } = [];
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public long? CustomerId { get; set; }
        public string Notes { get; set; }
        public bool IsDeferredPayment { get; set; }
        public decimal AmountPaid { get; set; }
    }

    public sealed class UpdateSalesInvoiceRequest
    {
        public string Notes { get; set; }
        public bool IsDeferredPayment { get; set; }
        public List<UpdateSalesInvoiceItemDto> Items { get; set; } = [];
    }
}
