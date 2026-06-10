using Application.Returns.Commands.CreateReturn;
using Application.Returns.Dtos;
using Application.Returns.Queries.GetReturnInvoice;
using Application.Returns.Queries.SearchReturnInvoices;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Store_Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/returns")]
public sealed class ReturnsController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateReturnRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateReturnCommand(
            request.SalesInvoiceId,
            request.ReturnReasonType,
            request.Notes ?? string.Empty,
            request.Items ?? []);

        var result = await sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string returnNumber,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new SearchReturnInvoicesQuery(dateFrom, dateTo, returnNumber ?? string.Empty, pageNumber, pageSize),
            cancellationToken);

        return Ok(new
        {
            items = result.Items,
            totalCount = result.TotalCount,
            pageNumber = result.PageNumber,
            pageSize = result.PageSize
        });
    }

    [HttpGet("{returnId}")]
    public async Task<IActionResult> Get(
        string returnId,
        CancellationToken cancellationToken)
    {
        if (!long.TryParse(returnId, out var id))
            return BadRequest(new { message = "معرف المرتجع غير صالح." });

        var result = await sender.Send(new GetReturnInvoiceQuery(id), cancellationToken);
        return Ok(result);
    }

    public sealed class CreateReturnRequest
    {
        public long SalesInvoiceId { get; set; }
        public int ReturnReasonType { get; set; }
        public string Notes { get; set; }
        public List<ReturnLineRequestDto> Items { get; set; } = [];
    }
}
