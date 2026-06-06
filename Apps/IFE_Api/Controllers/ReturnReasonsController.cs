using Application.ReturnReasons.Commands.CreateReturnReason;
using Application.ReturnReasons.Commands.DeleteReturnReason;
using Application.ReturnReasons.Commands.UpdateReturnReason;
using Application.ReturnReasons.Queries.GetReturnReasons;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Store_Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/return-reasons")]
public sealed class ReturnReasonsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var reasons = await sender.Send(new GetReturnReasonsQuery(), cancellationToken);
        return Ok(reasons);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] ReturnReasonRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateReturnReasonCommand(request.Name, request.IsReturnToStock),
            cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        string id,
        [FromBody] ReturnReasonRequest request,
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(id, out var reasonId))
            return BadRequest(new { message = "معرف السبب غير صالح." });

        await sender.Send(
            new UpdateReturnReasonCommand(reasonId, request.Name, request.IsReturnToStock),
            cancellationToken);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        string id,
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(id, out var reasonId))
            return BadRequest(new { message = "معرف السبب غير صالح." });

        await sender.Send(new DeleteReturnReasonCommand(reasonId), cancellationToken);
        return Ok();
    }

    public sealed class ReturnReasonRequest
    {
        public string Name { get; set; } = string.Empty;
        public bool IsReturnToStock { get; set; } = true;
    }
}
