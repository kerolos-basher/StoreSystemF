using Application.Abstractions.Persistence;
using Application.Products.Dtos;
using Domain.CategoryAggregate;

namespace Application.Categories.Commands.CreateCategory;

public sealed class CreateCategoryCommandHandler(
    IApplicationDbContext context,
    ISequenceService sequenceService)
    : ICommandHandler<CreateCategoryCommand, CategoryLookupDto>
{
    public async Task<CategoryLookupDto> Handle(
        CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var normalized = request.Name.Trim().ToLower();
        var existing = await context.Category
            .FirstOrDefaultAsync(c => c.Name.ToLower() == normalized, cancellationToken);

        if (existing is not null)
            return new CategoryLookupDto(existing.Id, existing.Name);

        var id = await sequenceService.GetNextValueAsync(SequenceKeys.CategorySequence, cancellationToken);
        var category = Category.Create(id, request.Name.Trim());
        context.Category.Add(category);
        await context.SaveChangesAsync();

        return new CategoryLookupDto(category.Id, category.Name);
    }
}
