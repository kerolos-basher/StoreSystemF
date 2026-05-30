using Domain.ProductAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasMany(p => p.ProductDetails)
            .WithOne(d => d.Product)
            .HasForeignKey(d => d.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(p => p.ProductDetails)
            .HasField("_productDetails")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
