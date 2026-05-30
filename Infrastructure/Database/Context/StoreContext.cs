
using Application.Abstractions.Persistence;
using Application.Abstractions.Services;
using Domain.CategoryAggregate;
using Domain.InventoryAggregate;
using Domain.ProductAggregate;
using Domain.SalesAggregate;
using Domain.SupplierAggregate;
using Infrastructure.Database.Configurations;
using Infrastructure.Services.QRCode;
using Infrastructure.Services.Sequences;
using System.Reflection;

namespace Infrastructure.Database.Context;

public class StoreContext : IdentityDbContext, IApplicationDbContext
{
    public delegate long? GetCurrentUserId();
    public StoreContext(DbContextOptions<StoreContext> options) : base(options)
    {
    }


    public virtual DbSet<Category> Category { get; set; }
    public virtual DbSet<Product> Product { get; set; }
    public virtual DbSet<ProductDetails> ProductDetails { get; set; }
    public virtual DbSet<Supplier> Supplier { get; set; }
    public virtual DbSet<SalesInvoice> SalesInvoice { get; set; }
    public virtual DbSet<SalesInvoiceItem> SalesInvoiceItem { get; set; }
    public virtual DbSet<InventoryTransaction> InventoryTransaction { get; set; }

    public virtual async Task<int> SaveChangesAsync(long? userId = null)
    {
        var result = base.SaveChangesAsync();
        return await result;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        builder.Properties<DateOnly>()
            .HaveConversion<DateOnlyConverter>()
            .HaveColumnType("date");
        base.ConfigureConventions(builder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            relationship.DeleteBehavior = DeleteBehavior.Restrict;


        base.OnModelCreating(modelBuilder);

        ApplySoftDeleteQueryFilter(modelBuilder);

        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new SalesInvoiceConfiguration());

        #region Sequence
        modelBuilder.HasSequence<long>(SequenceKeys.SupplierSequence)
            .StartsAt(1)
            .IncrementsBy(1);

        modelBuilder.HasSequence<long>(SequenceKeys.SalesInvoiceSequence)
            .StartsAt(1)
            .IncrementsBy(1);
        #endregion

    }



    private static void ApplySoftDeleteQueryFilter(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(StoreContext)
                    .GetMethod(nameof(SetSoftDeleteFilter),
                        BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);

                method.Invoke(null, new object[] { modelBuilder });
            }
        }
    }
    private static void SetSoftDeleteFilter<TEntity>(ModelBuilder modelBuilder)
    where TEntity : class, ISoftDelete
    {
        modelBuilder.Entity<TEntity>()
            .HasQueryFilter(x => !x.IsDeleted);
    }
}
