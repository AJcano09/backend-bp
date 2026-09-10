using ClientService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClientService.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent mapping of the derived Cliente entity (TPT - Table per Type).
/// ClienteId is the PK of the Clientes table and, at the same time, the FK
/// towards Personas.Id: exactly the schema declared in database.sql.
/// </summary>
public sealed class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
builder.ToTable("Clientes");

        // TPT inherits the key from the root entity Persona (Id column in both
        // tables). EF Core does not support per-table column renaming for
        // inherited keys, so ClienteId remains the conceptual/API attribute.
        // Maps to the Id column here + FK to Personas.Id, exactly as database.sql.

        builder.Property(c => c.Contrasena).HasMaxLength(255).IsRequired();
        builder.Property(c => c.Estado).IsRequired();

        // NOTE: TPT does not allow query filters on derived types.
        // Soft-delete filtering is handled explicitly in the repository (DRY helper).
    }
}