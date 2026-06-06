

using Domain.CategoryAggregate;
using Domain.InventoryAggregate;
using Domain.LookupAggregate;
using Domain.ProductAggregate;
using Domain.ReturnsAggregate;
using Domain.SalesAggregate;
using Domain.SupplierAggregate;


namespace Application.Abstractions.Persistence;

public interface IApplicationDbContext
{
    DbSet<Category> Category { get; }
    DbSet<Product> Product { get; }
    DbSet<ProductDetails> ProductDetails { get; }
    DbSet<Supplier> Supplier { get; }
    DbSet<SalesInvoice> SalesInvoice { get; }
    DbSet<SalesInvoiceItem> SalesInvoiceItem { get; }
    DbSet<InventoryTransaction> InventoryTransaction { get; }
    DbSet<ReturnReason> ReturnReason { get; }
    DbSet<ReturnInvoice> ReturnInvoice { get; }
    DbSet<ReturnInvoiceItem> ReturnInvoiceItem { get; }


    Task<int> SaveChangesAsync(long? userId = null);
}
