using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MicroMercado.Models;

namespace MicroMercado.Data.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("clients");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .UseIdentityColumn();

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.TaxDocument)
            .HasColumnName("tax_document")
            .HasMaxLength(20)
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
    }
}