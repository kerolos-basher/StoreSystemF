using Application.Returns.Commands.CreateReturn;
using Application.Returns.Dtos;
using Application.Returns.Queries.GetReturnInvoice;
using Application.Returns.Queries.SearchReturnInvoices;
using Infrastructure.Services.LogFile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Store_Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/returns")]
public sealed class ReturnsController : StoreBaseController
{
    private readonly ISender _sender;

    public ReturnsController(LogFileService logger, ISender sender) : base(logger) => _sender = sender;

    [HttpPost]
    public Task<IActionResult> Create([FromBody] CreateReturnRequest request, CancellationToken cancellationToken) =>
        TryCatchLogAsync(async () =>
        {
            var command = new CreateReturnCommand(
                request.SalesInvoiceId,
                request.ReturnReasonType,
                request.Notes ?? string.Empty,
                request.Items ?? []);

            var result = await _sender.Send(command, cancellationToken);
            return Ok(result);
        });

    [HttpGet]
    public Task<IActionResult> Search(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string returnNumber,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default) =>
        TryCatchLogAsync(async () =>
        {
            var result = await _sender.Send(
                new SearchReturnInvoicesQuery(dateFrom, dateTo, returnNumber ?? string.Empty, pageNumber, pageSize),
                cancellationToken);

            return Ok(new
            {
                items = result.Items,
                totalCount = result.TotalCount,
                pageNumber = result.PageNumber,
                pageSize = result.PageSize
            });
        });

    [HttpGet("{returnId}")]
    public Task<IActionResult> Get(string returnId, CancellationToken cancellationToken) =>
        TryCatchLogAsync(async () =>
        {
            if (!long.TryParse(returnId, out var id))
                return BadRequest(new { message = "معرف المرتجع غير صالح." });

            var result = await _sender.Send(new GetReturnInvoiceQuery(id), cancellationToken);
            return Ok(result);
        });

    public sealed class CreateReturnRequest
    {
        public long SalesInvoiceId { get; set; }
        public int ReturnReasonType { get; set; }
        public string Notes { get; set; }
        public List<ReturnLineRequestDto> Items { get; set; } = [];
    }
}
