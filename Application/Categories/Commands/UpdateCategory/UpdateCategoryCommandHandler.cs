using Application.Abstractions.Persistence;

namespace Application.Categories.Commands.UpdateCategory;

public sealed class UpdateCategoryCommandHandler(IApplicationDbContext context)
    : ICommandHandler<UpdateCategoryCommand>
{
    public async Task Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await context.Category
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new Exception("الفئة غير موجودة.");

        category.Update(request.Name);
        await context.SaveChangesAsync();
    }
}
