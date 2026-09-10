using AccountService.Infrastructure.Persistence.ReadModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccountService.Infrastructure.Persistence.Configurations;

/// <summary>Fluent mapping of the client read model. Matches database.sql.</summary>
public sealed class ClientesLecturaConfiguration : IEntityTypeConfiguration<ClientesLectura>
{
    public void Configure(EntityTypeBuilder<ClientesLectura> builder)
    {
        builder.ToTable("ClientesLectura");
        builder.HasKey(l => l.ClienteId);
        builder.Property(l => l.Nombre).HasMaxLength(150).IsRequired();
        // Default true keeps the seed script (BaseDatos.sql) valid: a
        // pre-existing row without Estado is an active client.
        builder.Property(l => l.Estado).IsRequired().HasDefaultValue(true);
    }
}