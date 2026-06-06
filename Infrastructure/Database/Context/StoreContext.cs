
using Application.Abstractions.Persistence;
using Application.Abstractions.Services;
using Domain.CategoryAggregate;
using Domain.InventoryAggregate;
using Domain.LookupAggregate;
using Domain.ProductAggregate;
using Domain.ReturnsAggregate;
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
    public virtual DbSet<ReturnReason> ReturnReason { get; set; }
    public virtual DbSet<ReturnInvoice> ReturnInvoice { get; set; }
    public virtual DbSet<ReturnInvoiceItem> ReturnInvoiceItem { get; set; }

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
        modelBuilder.ApplyConfiguration(new ReturnInvoiceConfiguration());

        modelBuilder.Entity<ReturnReason>(entity =>
        {
            entity.ToTable("ReturnReason");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(100);
            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(x => x.ProductName).HasMaxLength(200);
        });

        modelBuilder.Entity<ProductDetails>(entity =>
        {
            entity.Property(x => x.BarCode).HasMaxLength(100);
            entity.Property(x => x.Notes).HasMaxLength(500);
        });

        modelBuilder.Entity<SalesInvoice>(entity =>
        {
            entity.Property(x => x.InvoiceNumber).HasMaxLength(50);
        });

        modelBuilder.Entity<InventoryTransaction>(entity =>
        {
            entity.Property(x => x.TransactionType)
                .HasConversion<int>();
            entity.Property(x => x.Reference).HasMaxLength(100);
        });

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
