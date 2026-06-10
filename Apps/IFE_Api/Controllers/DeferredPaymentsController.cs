using Application.DeferredPayments.Commands.UpdateDeferredPayment;
using Application.DeferredPayments.Queries.SearchDeferredPayments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Store_Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/deferred-payments")]
public sealed class DeferredPaymentsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string customerTerm,
        [FromQuery] string customerName,
        [FromQuery] bool? isFullyPaid,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var term = !string.IsNullOrWhiteSpace(customerTerm) ? customerTerm : customerName;
        var result = await sender.Send(
            new SearchDeferredPaymentsQuery(term, isFullyPaid, pageNumber, pageSize),
            cancellationToken);

        return Ok(new
        {
            items = result.Items,
            totalCount = result.TotalCount,
            pageNumber = result.PageNumber,
            pageSize = result.PageSize
        });
    }

    [HttpGet("{id}/statement")]
    public async Task<IActionResult> GetStatement(
        long id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new Application.DeferredPayments.Queries.GetDeferredPaymentStatement.GetDeferredPaymentStatementQuery(id),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id}/payments")]
    public async Task<IActionResult> RegisterPayment(
        long id,
        [FromBody] RegisterPaymentRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new UpdateDeferredPaymentCommand(id, request.AmountPaid, request.Notes),
            cancellationToken);

        return Ok();
    }

    public sealed class RegisterPaymentRequest
    {
        public decimal AmountPaid { get; set; }
        public string Notes { get; set; }
    }
}
