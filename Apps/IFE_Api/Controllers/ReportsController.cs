using Application.Reports.Queries.GetFinancialReport;
using Infrastructure.Services.LogFile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Store_Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/reports")]
public sealed class ReportsController : StoreBaseController
{
    private readonly ISender _sender;

    public ReportsController(LogFileService logger, ISender sender) : base(logger) => _sender = sender;

    [HttpGet("financial")]
    public Task<IActionResult> GetFinancial(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken = default) =>
        TryCatchLogAsync(async () =>
            Ok(await _sender.Send(new GetFinancialReportQuery(from, to), cancellationToken)));
}
