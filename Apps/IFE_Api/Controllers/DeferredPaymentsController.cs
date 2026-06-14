using Application.DeferredPayments.Commands.DeleteDeferredPaymentTransaction;
using Application.DeferredPayments.Commands.UpdateDeferredPayment;
using Application.DeferredPayments.Commands.UpdateDeferredPaymentTransaction;
using Application.DeferredPayments.Queries.SearchDeferredPayments;
using Infrastructure.Services.LogFile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Store_Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/deferred-payments")]
public sealed class DeferredPaymentsController : StoreBaseController
{
    private readonly ISender _sender;

    public DeferredPaymentsController(LogFileService logger, ISender sender) : base(logger) => _sender = sender;

    [HttpGet]
    public Task<IActionResult> Search(
        [FromQuery] string customerTerm,
        [FromQuery] string customerName,
        [FromQuery] bool? isFullyPaid,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        TryCatchLogAsync(async () =>
        {
            var term = !string.IsNullOrWhiteSpace(customerTerm) ? customerTerm : customerName;
            var result = await _sender.Send(
                new SearchDeferredPaymentsQuery(term, isFullyPaid, pageNumber, pageSize),
                cancellationToken);

            return Ok(new
            {
                items = result.Items,
                totalCount = result.TotalCount,
                pageNumber = result.PageNumber,
                pageSize = result.PageSize
            });
        });

    [HttpGet("{id}/statement")]
    public Task<IActionResult> GetStatement(long id, CancellationToken cancellationToken) =>
        TryCatchLogAsync(async () =>
            Ok(await _sender.Send(
                new Application.DeferredPayments.Queries.GetDeferredPaymentStatement.GetDeferredPaymentStatementQuery(id),
                cancellationToken)));

    [HttpPost("{id}/payments")]
    public Task<IActionResult> RegisterPayment(
        long id,
        [FromBody] RegisterPaymentRequest request,
        CancellationToken cancellationToken) =>
        TryCatchLogAsync(async () =>
        {
            await _sender.Send(new UpdateDeferredPaymentCommand(id, request.AmountPaid, request.Notes), cancellationToken);
            return Ok();
        });

    [HttpPut("{id}/payments/{transactionId}")]
    public Task<IActionResult> UpdatePayment(
        long id,
        long transactionId,
        [FromBody] RegisterPaymentRequest request,
        CancellationToken cancellationToken) =>
        TryCatchLogAsync(async () =>
        {
            await _sender.Send(
                new UpdateDeferredPaymentTransactionCommand(id, transactionId, request.AmountPaid, request.Notes),
                cancellationToken);
            return Ok();
        });

    [HttpDelete("{id}/payments/{transactionId}")]
    public Task<IActionResult> DeletePayment(long id, long transactionId, CancellationToken cancellationToken) =>
        TryCatchLogAsync(async () =>
        {
            await _sender.Send(new DeleteDeferredPaymentTransactionCommand(id, transactionId), cancellationToken);
            return Ok();
        });

    public sealed class RegisterPaymentRequest
    {
        public decimal AmountPaid { get; set; }
        public string Notes { get; set; }
    }
}
