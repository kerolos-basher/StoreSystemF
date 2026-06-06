namespace Application.Categories.Commands.UpdateCategory;

public sealed record UpdateCategoryCommand(long Id, string Name) : ICommand;
