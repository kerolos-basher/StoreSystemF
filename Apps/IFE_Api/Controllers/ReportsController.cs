using Application.Reports.Queries.GetFinancialReport;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Store_Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/reports")]
public sealed class ReportsController(ISender sender) : ControllerBase
{
    [HttpGet("financial")]
    public async Task<IActionResult> GetFinancial(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetFinancialReportQuery(from, to), cancellationToken);
        return Ok(result);
    }
}
