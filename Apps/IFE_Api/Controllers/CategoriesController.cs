using Application.Categories.Commands.CreateCategory;
using Application.Categories.Commands.DeleteCategory;
using Application.Categories.Commands.UpdateCategory;
using Application.Lookups.Queries.GetCategories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Store_Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/categories")]
public sealed class CategoriesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var categories = await sender.Send(new GetCategoriesQuery(), cancellationToken);
        return Ok(categories);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] LookupRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateCategoryCommand(request.Name), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        string id,
        [FromBody] LookupRequest request,
        CancellationToken cancellationToken)
    {
        if (!long.TryParse(id, out var categoryId))
            return BadRequest(new { message = "معرف الفئة غير صالح." });

        await sender.Send(new UpdateCategoryCommand(categoryId, request.Name), cancellationToken);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        string id,
        CancellationToken cancellationToken)
    {
        if (!long.TryParse(id, out var categoryId))
            return BadRequest(new { message = "معرف الفئة غير صالح." });

        await sender.Send(new DeleteCategoryCommand(categoryId), cancellationToken);
        return Ok();
    }

    public sealed class LookupRequest
    {
        public string Name { get; set; } = string.Empty;
    }
}
