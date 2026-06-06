using Application.Products.Dtos;

namespace Application.Categories.Commands.CreateCategory;

public sealed record CreateCategoryCommand(string Name) : ICommand<CategoryLookupDto>;
