using Application.Abstractions.Persistence;

namespace Application.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandHandler(IApplicationDbContext context)
    : ICommandHandler<UpdateProductCommand>
{
    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await context.Product
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken)
            ?? throw new Exception("المنتج غير موجود.");

        product.Update(request.ProductName);
        await context.SaveChangesAsync();
    }
}
