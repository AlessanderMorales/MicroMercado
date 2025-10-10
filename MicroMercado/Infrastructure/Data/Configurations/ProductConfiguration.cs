using MicroMercado.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroMercado.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");
        
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasColumnName("id")
            .UseIdentityColumn();
        
        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(50)
            .IsRequired();
        
        builder.Property(p => p.Description)
            .HasColumnName("description")
            .HasMaxLength(50);
        
        builder.Property(p => p.Brand)
            .HasColumnName("brand")
            .HasMaxLength(20);
        
        builder.Property(p => p.Price)
            .HasColumnName("price")
            .HasColumnType("decimal(6,2)");
        
        builder.Property(p => p.Stock)
            .HasColumnName("stock")
            .HasDefaultValue(0);
        
        builder.Property(p => p.CategoryId)
            .HasColumnName("category_id");
        
        builder.Property(p => p.Status)
            .HasColumnName("status")
            .HasDefaultValue(1);
        
        builder.Property(p => p.LastUpdate)
            .HasColumnName("last_update")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}