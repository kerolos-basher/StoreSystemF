using Application.Sales.Commands.CreateSale;
using Application.Sales.Commands.DeleteSalesInvoice;
using Application.Sales.Commands.UpdateSalesInvoice;
using Application.Sales.Dtos;
using Application.Sales.Queries.GetSalesInvoice;
using Application.Sales.Queries.GetSalesInvoiceByNumber;
using Application.Sales.Queries.SearchSalesInvoices;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Store_Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/sales")]
public sealed class SalesController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateSaleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateSaleCommand(
            request.Items ?? [],
            request.CustomerName,
            request.CustomerPhone,
            request.CustomerId,
            request.Notes ?? string.Empty,
            request.IsDeferredPayment,
            request.AmountPaid);

        var result = await sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet("invoices")]
    public async Task<IActionResult> SearchInvoices(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string invoiceNumber,
        [FromQuery] string customerTerm,
        [FromQuery] bool? isDeferredPayment,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
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
    }

    [HttpGet("invoices/by-number/{number}")]
    public async Task<IActionResult> GetByNumber(
        string number,
        CancellationToken cancellationToken)
    {
        var invoice = await sender.Send(new GetSalesInvoiceByNumberQuery(number), cancellationToken);
        if (invoice is null)
            return NotFound(new { message = "الفاتورة غير موجودة." });

        return Ok(invoice);
    }

    [HttpGet("{invoiceId}")]
    public async Task<IActionResult> GetInvoice(
        string invoiceId,
        CancellationToken cancellationToken)
    {
        if (!long.TryParse(invoiceId, out var id))
            return BadRequest(new { message = "معرف الفاتورة غير صالح." });

        var invoice = await sender.Send(new GetSalesInvoiceQuery(id), cancellationToken);
        return Ok(invoice);
    }

    [HttpPut("{invoiceId}")]
    public async Task<IActionResult> UpdateInvoice(
        long invoiceId,
        [FromBody] UpdateSalesInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new UpdateSalesInvoiceCommand(
                invoiceId,
                request.Notes,
                request.IsDeferredPayment,
                request.Items ?? []),
            cancellationToken);

        return Ok();
    }

    [HttpDelete("{invoiceId}")]
    public async Task<IActionResult> DeleteInvoice(
        long invoiceId,
        CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteSalesInvoiceCommand(invoiceId), cancellationToken);
        return Ok();
    }

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
