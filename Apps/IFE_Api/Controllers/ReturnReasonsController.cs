using Application.ReturnReasons.Commands.CreateReturnReason;
using Application.ReturnReasons.Commands.DeleteReturnReason;
using Application.ReturnReasons.Commands.UpdateReturnReason;
using Application.ReturnReasons.Queries.GetReturnReasons;
using Infrastructure.Services.LogFile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Store_Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/return-reasons")]
public sealed class ReturnReasonsController : StoreBaseController
{
    private readonly ISender _sender;

    public ReturnReasonsController(LogFileService logger, ISender sender) : base(logger) => _sender = sender;

    [HttpGet]
    public Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        TryCatchLogAsync(async () => Ok(await _sender.Send(new GetReturnReasonsQuery(), cancellationToken)));

    [HttpPost]
    public Task<IActionResult> Create([FromBody] ReturnReasonRequest request, CancellationToken cancellationToken) =>
        TryCatchLogAsync(async () =>
            Ok(await _sender.Send(new CreateReturnReasonCommand(request.Name, request.IsReturnToStock), cancellationToken)));

    [HttpPut("{id}")]
    public Task<IActionResult> Update(string id, [FromBody] ReturnReasonRequest request, CancellationToken cancellationToken) =>
        TryCatchLogAsync(async () =>
        {
            if (!int.TryParse(id, out var reasonId))
                return BadRequest(new { message = "معرف السبب غير صالح." });

            await _sender.Send(new UpdateReturnReasonCommand(reasonId, request.Name, request.IsReturnToStock), cancellationToken);
            return Ok();
        });

    [HttpDelete("{id}")]
    public Task<IActionResult> Delete(string id, CancellationToken cancellationToken) =>
        TryCatchLogAsync(async () =>
        {
            if (!int.TryParse(id, out var reasonId))
                return BadRequest(new { message = "معرف السبب غير صالح." });

            await _sender.Send(new DeleteReturnReasonCommand(reasonId), cancellationToken);
            return Ok();
        });

    public sealed class ReturnReasonRequest
    {
        public string Name { get; set; } = string.Empty;
        public bool IsReturnToStock { get; set; } = true;
    }
}
