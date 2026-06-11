using Application.Suppliers.Commands.CreateSupplier;
using Application.Suppliers.Commands.DeleteSupplier;
using Application.Suppliers.Commands.UpdateSupplier;
using Application.Lookups.Queries.SearchSuppliers;
using Infrastructure.Services.LogFile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Store_Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/suppliers")]
public sealed class SuppliersController : StoreBaseController
{
    private readonly ISender _sender;

    public SuppliersController(LogFileService logger, ISender sender) : base(logger) => _sender = sender;

    [HttpGet]
    public Task<IActionResult> Search([FromQuery] string term, CancellationToken cancellationToken) =>
        TryCatchLogAsync(async () =>
            Ok(await _sender.Send(new SearchSuppliersQuery(term ?? string.Empty), cancellationToken)));

    [HttpPost]
    public Task<IActionResult> Create([FromBody] CreateSupplierRequest request, CancellationToken cancellationToken) =>
        TryCatchLogAsync(async () =>
            Ok(await _sender.Send(new CreateSupplierCommand(request.Name), cancellationToken)));

    [HttpPut("{id}")]
    public Task<IActionResult> Update(string id, [FromBody] CreateSupplierRequest request, CancellationToken cancellationToken) =>
        TryCatchLogAsync(async () =>
        {
            if (!long.TryParse(id, out var supplierId))
                return BadRequest(new { message = "معرف المورد غير صالح." });

            await _sender.Send(new UpdateSupplierCommand(supplierId, request.Name), cancellationToken);
            return Ok();
        });

    [HttpDelete("{id}")]
    public Task<IActionResult> Delete(string id, CancellationToken cancellationToken) =>
        TryCatchLogAsync(async () =>
        {
            if (!long.TryParse(id, out var supplierId))
                return BadRequest(new { message = "معرف المورد غير صالح." });

            await _sender.Send(new DeleteSupplierCommand(supplierId), cancellationToken);
            return Ok();
        });

    public sealed class CreateSupplierRequest
    {
        public string Name { get; set; } = string.Empty;
    }
}
