using Application.Categories.Commands.CreateCategory;
using Application.Categories.Commands.DeleteCategory;
using Application.Categories.Commands.UpdateCategory;
using Application.Lookups.Queries.GetCategories;
using Infrastructure.Services.LogFile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Store_Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/categories")]
public sealed class CategoriesController : StoreBaseController
{
    private readonly ISender _sender;

    public CategoriesController(LogFileService logger, ISender sender) : base(logger) => _sender = sender;

    [HttpGet]
    public Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        TryCatchLogAsync(async () => Ok(await _sender.Send(new GetCategoriesQuery(), cancellationToken)));

    [HttpPost]
    public Task<IActionResult> Create([FromBody] LookupRequest request, CancellationToken cancellationToken) =>
        TryCatchLogAsync(async () => Ok(await _sender.Send(new CreateCategoryCommand(request.Name), cancellationToken)));

    [HttpPut("{id}")]
    public Task<IActionResult> Update(string id, [FromBody] LookupRequest request, CancellationToken cancellationToken) =>
        TryCatchLogAsync(async () =>
        {
            if (!long.TryParse(id, out var categoryId))
                return BadRequest(new { message = "معرف الفئة غير صالح." });

            await _sender.Send(new UpdateCategoryCommand(categoryId, request.Name), cancellationToken);
            return Ok();
        });

    [HttpDelete("{id}")]
    public Task<IActionResult> Delete(string id, CancellationToken cancellationToken) =>
        TryCatchLogAsync(async () =>
        {
            if (!long.TryParse(id, out var categoryId))
                return BadRequest(new { message = "معرف الفئة غير صالح." });

            await _sender.Send(new DeleteCategoryCommand(categoryId), cancellationToken);
            return Ok();
        });

    public sealed class LookupRequest
    {
        public string Name { get; set; } = string.Empty;
    }
}
