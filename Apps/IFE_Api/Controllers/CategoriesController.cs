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
}
