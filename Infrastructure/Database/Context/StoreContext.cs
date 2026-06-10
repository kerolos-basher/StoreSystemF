
using Application.Abstractions.Persistence;
using Domain;
using Domain.CategoryAggregate;
using Domain.CustomerAggregate;
using Domain.DeferredPaymentAggregate;
using Domain.Events;
using Domain.InventoryAggregate;
using Domain.LookupAggregate;
using Domain.ProductAggregate;
using Domain.ReturnsAggregate;
using Domain.SalesAggregate;
using Domain.SupplierAggregate;
using Infrastructure.Database.Configurations;
using MediatR;
using System.Reflection;

namespace Infrastructure.Database.Context;

public class StoreContext : IdentityDbContext, IApplicationDbContext
{
    private readonly IPublisher? _publisher;

    public delegate long? GetCurrentUserId();

    public StoreContext(DbContextOptions<StoreContext> options, IPublisher publisher = null) : base(options)
    {
        _publisher = publisher;
    }

    public virtual DbSet<Category> Category { get; set; }
    public virtual DbSet<Product> Product { get; set; }
    public virtual DbSet<ProductDetails> ProductDetails { get; set; }
    public virtual DbSet<Supplier> Supplier { get; set; }
    public virtual DbSet<Customer> Customer { get; set; }
    public virtual DbSet<SalesInvoice> SalesInvoice { get; set; }
    public virtual DbSet<SalesInvoiceItem> SalesInvoiceItem { get; set; }
    public virtual DbSet<DeferredPayment> DeferredPayment { get; set; }
    public virtual DbSet<DeferredPaymentTransaction> DeferredPaymentTransaction { get; set; }
    public virtual DbSet<InventoryTransaction> InventoryTransaction { get; set; }
    public virtual DbSet<ReturnReason> ReturnReason { get; set; }
    public virtual DbSet<ReturnInvoice> ReturnInvoice { get; set; }
    public virtual DbSet<ReturnInvoiceItem> ReturnInvoiceItem { get; set; }

    public virtual async Task<int> SaveChangesAsync(long? userId = null)
    {
        var domainEvents = ChangeTracker.Entries<ParentEntity>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        var result = await base.SaveChangesAsync();

        if (_publisher is not null)
        {
            foreach (var domainEvent in domainEvents)
                await _publisher.Publish(domainEvent);
        }

        foreach (var entry in ChangeTracker.Entries<ParentEntity>())
            entry.Entity.ClearDomainEvents();

        return result;
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

        ConfigureSequences(modelBuilder);
        ApplySoftDeleteQueryFilter(modelBuilder);

        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new SalesInvoiceConfiguration());
        modelBuilder.ApplyConfiguration(new ReturnInvoiceConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new DeferredPaymentConfiguration());
        modelBuilder.ApplyConfiguration(new DeferredPaymentTransactionConfiguration());

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
            entity.Property(x => x.IsDeferredPayment).HasDefaultValue(false);
            entity.HasOne<Customer>()
                .WithMany()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InventoryTransaction>(entity =>
        {
            entity.Property(x => x.TransactionType)
                .HasConversion<int>();
            entity.Property(x => x.Reference).HasMaxLength(100);
        });

    }

    private static void ConfigureSequences(ModelBuilder modelBuilder)
    {
        modelBuilder.HasSequence<long>(SequenceKeys.CategorySequence);
        modelBuilder.HasSequence<long>(SequenceKeys.SupplierSequence);
        modelBuilder.HasSequence<long>(SequenceKeys.ProductSequence);
        modelBuilder.HasSequence<long>(SequenceKeys.ProductDetailsSequence);
        modelBuilder.HasSequence<long>(SequenceKeys.CustomerSequence);
        modelBuilder.HasSequence<long>(SequenceKeys.SalesInvoiceSequence);
        modelBuilder.HasSequence<long>(SequenceKeys.SalesInvoiceItemSequence);
        modelBuilder.HasSequence<long>(SequenceKeys.ReturnInvoiceSequence);
        modelBuilder.HasSequence<long>(SequenceKeys.ReturnInvoiceItemSequence);
        modelBuilder.HasSequence<long>(SequenceKeys.DeferredPaymentSequence);
        modelBuilder.HasSequence<long>(SequenceKeys.DeferredPaymentTransactionSequence);
        modelBuilder.HasSequence<long>(SequenceKeys.InventoryTransactionSequence);
        modelBuilder.HasSequence<int>(SequenceKeys.ReturnReasonSequence);
        modelBuilder.HasSequence<long>(SequenceKeys.BarCodeSequence);

        UseSequenceId<Category>(modelBuilder);
        UseSequenceId<Supplier>(modelBuilder);
        UseSequenceId<Product>(modelBuilder);
        UseSequenceId<ProductDetails>(modelBuilder);
        UseSequenceId<Customer>(modelBuilder);
        UseSequenceId<SalesInvoice>(modelBuilder);
        UseSequenceId<SalesInvoiceItem>(modelBuilder);
        UseSequenceId<ReturnInvoice>(modelBuilder);
        UseSequenceId<ReturnInvoiceItem>(modelBuilder);
        UseSequenceId<DeferredPayment>(modelBuilder);
        UseSequenceId<DeferredPaymentTransaction>(modelBuilder);
        UseSequenceId<InventoryTransaction>(modelBuilder);
        UseSequenceId<ReturnReason, int>(modelBuilder);
    }

    private static void UseSequenceId<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class =>
        modelBuilder.Entity<TEntity>()
            .Property<long>("Id")
            .ValueGeneratedNever();

    private static void UseSequenceId<TEntity, TKey>(ModelBuilder modelBuilder)
        where TEntity : class =>
        modelBuilder.Entity<TEntity>()
            .Property<TKey>("Id")
            .ValueGeneratedNever();

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
