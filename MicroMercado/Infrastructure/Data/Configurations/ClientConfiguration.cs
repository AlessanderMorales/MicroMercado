using MicroMercado.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroMercado.Infrastructure.Data.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("clients");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .UseIdentityColumn();
        builder.Property(c => c.BusinessName)
            .HasColumnName("business_name")
            .HasMaxLength(150) 
            .IsRequired();

        builder.Property(c => c.Email)
            .HasColumnName("email")
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(c => c.Address)
            .HasColumnName("address")
            .HasMaxLength(150)
            .IsRequired(false);

        builder.Property(c => c.TaxDocument)
            .HasColumnName("tax_document")
            .HasMaxLength(25)
            .IsRequired();

        builder.Property(c => c.Status)
            .HasColumnName("status")
            .HasDefaultValue(1);

        builder.Property(c => c.LastUpdate)
            .HasColumnName("last_update")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(c => c.TaxDocument)
            .IsUnique();

        builder.HasIndex(c => c.Email)
            .IsUnique();
    }
}