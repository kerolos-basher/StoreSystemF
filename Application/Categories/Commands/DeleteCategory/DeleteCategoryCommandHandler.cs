using Application.Abstractions.Persistence;

namespace Application.Categories.Commands.DeleteCategory;

public sealed class DeleteCategoryCommandHandler(IApplicationDbContext context)
    : ICommandHandler<DeleteCategoryCommand>
{
    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await context.Category
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new Exception("الفئة غير موجودة.");

        category.SoftDelete();
        await context.SaveChangesAsync();
    }
}
