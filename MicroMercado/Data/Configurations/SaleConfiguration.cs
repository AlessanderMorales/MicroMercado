using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MicroMercado.Models;

namespace MicroMercado.Data.Configurations;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("sales");
        
        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.Id)
            .HasColumnName("id")
            .UseIdentityColumn();
        
        builder.Property(s => s.UserId)
            .HasColumnName("user_id")
            .IsRequired();
        
        builder.Property(s => s.SaleDate)
            .HasColumnName("sale_date")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        builder.Property(s => s.TotalAmount)
            .HasColumnName("total_amount")
            .HasColumnType("decimal(8,2)")
            .IsRequired();
        
        builder.Property(s => s.ClientId)
            .HasColumnName("client_id")
            .IsRequired(false);
        
        builder.HasOne(s => s.Client)
            .WithMany(c => c.Sales)
            .HasForeignKey(s => s.ClientId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasIndex(s => s.SaleDate);
        builder.HasIndex(s => s.ClientId);
        builder.HasIndex(s => s.UserId);
    }
}