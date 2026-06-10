using Domain.DeferredPaymentAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

public sealed class DeferredPaymentConfiguration : IEntityTypeConfiguration<DeferredPayment>
{
    public void Configure(EntityTypeBuilder<DeferredPayment> builder)
    {
        builder.ToTable("DeferredPayment");
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.PaidAmount).HasPrecision(18, 2);
        builder.Property(x => x.RemainingAmount).HasPrecision(18, 2);
        builder.Property(x => x.Notes).HasMaxLength(500);
        builder.HasOne<Domain.SalesAggregate.SalesInvoice>()
            .WithMany()
            .HasForeignKey(x => x.SalesInvoiceId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Domain.CustomerAggregate.Customer>()
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.Transactions)
            .WithOne()
            .HasForeignKey(x => x.DeferredPaymentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(x => x.Transactions)
            .HasField("_transactions")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class DeferredPaymentTransactionConfiguration : IEntityTypeConfiguration<DeferredPaymentTransaction>
{
    public void Configure(EntityTypeBuilder<DeferredPaymentTransaction> builder)
    {
        builder.ToTable("DeferredPaymentTransaction");
        builder.Property(x => x.AmountPaid).HasPrecision(18, 2);
        builder.Property(x => x.Notes).HasMaxLength(500);
    }
}
