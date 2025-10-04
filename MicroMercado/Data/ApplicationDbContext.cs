using Microsoft.EntityFrameworkCore;
using MicroMercado.Models;

namespace MicroMercado.Data;
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    
    public DbSet<Client> Clients { get; set; }
    public DbSet<Sale> Sales { get; set; }
    public DbSet<SaleItem> SaleItems { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
    
    public override int SaveChanges()
    {
        UpdateLastUpdateTimestamp();
        return base.SaveChanges();
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateLastUpdateTimestamp();
        return base.SaveChangesAsync(cancellationToken);
    }
    
    private void UpdateLastUpdateTimestamp()
    {
        var modifiedEntries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified);
        
        foreach (var entry in modifiedEntries)
        {
            var lastUpdateProperty = entry.Entity.GetType().GetProperty("LastUpdate");
            if (lastUpdateProperty != null && lastUpdateProperty.PropertyType == typeof(DateTime))
            {
                lastUpdateProperty.SetValue(entry.Entity, DateTime.UtcNow);
            }
        }
    }
}