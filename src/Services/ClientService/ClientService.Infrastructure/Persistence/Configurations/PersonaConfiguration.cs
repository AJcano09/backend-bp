using ClientService.Domain.Entities;
using ClientService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ClientService.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent mapping of the Persona base entity.
/// TPT: this table is the hierarchy root.
/// </summary>
public sealed class PersonaConfiguration : IEntityTypeConfiguration<Persona>
{
    public void Configure(EntityTypeBuilder<Persona> builder)
    {
        builder.ToTable("Personas");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd();

        builder.Property(p => p.Nombre).HasMaxLength(150).IsRequired();
        builder.Property(p => p.Genero).HasMaxLength(20).IsRequired();
        builder.Property(p => p.Edad).IsRequired();
        builder.Property(p => p.Identificacion).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Direccion).HasMaxLength(255);
        builder.Property(p => p.Telefono)
            .HasConversion(new ValueConverter<Telefono, string>(
                t => t.Value,
                value => Telefono.Create(value)))
            .HasMaxLength(20);

        // Same CHECK as database.sql: age must be a valid domain value.
        builder.ToTable(t => t.HasCheckConstraint("CK_Personas_Age", "\"Edad\" BETWEEN 1 AND 130"));

        // Business key + mitigation of sequential-id enumeration.
        builder.HasIndex(p => p.Identificacion).IsUnique();
    }
}