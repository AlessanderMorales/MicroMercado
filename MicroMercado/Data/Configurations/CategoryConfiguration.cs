using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MicroMercado.Models;

namespace MicroMercado.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");
        
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasColumnName("id")
            .UseIdentityColumn();
        
        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(20)
            .IsRequired();
        
        builder.Property(c => c.Status)
            .HasColumnName("status")
            .HasDefaultValue(1);
        
        builder.Property(c => c.LastUpdate)
            .HasColumnName("last_update")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}