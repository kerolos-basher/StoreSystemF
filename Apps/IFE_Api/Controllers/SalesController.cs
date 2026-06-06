using Application.Sales.Commands.CreateSale;
using Application.Sales.Dtos;
using Application.Sales.Queries.GetSalesInvoice;
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
            request.Discount,
            request.Tax,
            request.Notes ?? string.Empty,
            request.CustomerId);

        var result = await sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet("invoices")]
    public async Task<IActionResult> SearchInvoices(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string invoiceNumber,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new SearchSalesInvoicesQuery(dateFrom, dateTo, invoiceNumber ?? string.Empty, pageNumber, pageSize),
            cancellationToken);

        return Ok(new
        {
            items = result.Items,
            totalCount = result.TotalCount,
            pageNumber = result.PageNumber,
            pageSize = result.PageSize
        });
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

    public sealed class CreateSaleRequest
    {
        public List<SaleLineRequestDto> Items { get; set; } = [];
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public string Notes { get; set; }
        public long? CustomerId { get; set; }
    }
}
