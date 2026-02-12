using Microsoft.EntityFrameworkCore;

namespace Coupa.Supplier.Infrastructure.Persistence;

public sealed class SupplierDbContext(DbContextOptions<SupplierDbContext> options) : DbContext(options)
{
    public DbSet<CoupaSupplierRow> Suppliers => Set<CoupaSupplierRow>();
    public DbSet<CoupaErrorLogRow> ErrorLogs => Set<CoupaErrorLogRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CoupaSupplierRow>().HasKey(x => x.StgId);
        modelBuilder.Entity<CoupaErrorLogRow>().HasKey(x => x.Id);
        base.OnModelCreating(modelBuilder);
    }
}
