using Application.Abstractions.Persistence;

namespace Application.Products.Commands.DeleteProduct;

public sealed class DeleteProductCommandHandler(IApplicationDbContext context)
    : ICommandHandler<DeleteProductCommand>
{
    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await context.Product
            .Include(p => p.ProductDetails)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken)
            ?? throw new Exception("المنتج غير موجود.");

        product.SoftDelete();
        await context.SaveChangesAsync();
    }
}
