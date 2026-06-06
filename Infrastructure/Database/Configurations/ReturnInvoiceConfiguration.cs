using Domain.ReturnsAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

public sealed class ReturnInvoiceConfiguration : IEntityTypeConfiguration<ReturnInvoice>
{
    public void Configure(EntityTypeBuilder<ReturnInvoice> builder)
    {
        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.ReturnInvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(x => x.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_items");
    }
}
