using MicroMercado.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroMercado.Infrastructure.Data.Configurations;

public class SaleItemConfiguration  : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("sale_items");
        
        builder.HasKey(si => new { si.SaleId, si.ProductId });
        
        builder.Property(si => si.SaleId)
            .HasColumnName("sale_id");
        
        builder.Property(si => si.ProductId)
            .HasColumnName("product_id");
        
        builder.Property(si => si.Quantity)
            .HasColumnName("quantity")
            .IsRequired();
        
        builder.Property(si => si.Price)
            .HasColumnName("price")
            .HasColumnType("decimal(6,2)")
            .IsRequired();
        
        builder.HasOne(si => si.Sale)
            .WithMany(s => s.SaleItems)
            .HasForeignKey(si => si.SaleId)
            .OnDelete(DeleteBehavior.Cascade); // Si se borra la venta, se borran sus items
        
        builder.HasOne(si => si.Product)
            .WithMany()
            .HasForeignKey(si => si.ProductId)
            .OnDelete(DeleteBehavior.Restrict); // No se puede borrar producto si tiene ventas
    }
}