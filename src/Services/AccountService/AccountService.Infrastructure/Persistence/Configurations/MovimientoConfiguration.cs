using AccountService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccountService.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent mapping of the immutable movements ledger. Matches database.sql:
/// identity PK, enum stored as text, numeric(18,2), FK to Cuentas.
/// </summary>
public sealed class MovimientoConfiguration : IEntityTypeConfiguration<Movimiento>
{
    public void Configure(EntityTypeBuilder<Movimiento> builder)
    {
        builder.ToTable("Movimientos");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Fecha)
            .HasColumnType("timestamp")
            .IsRequired();

        builder.Property(m => m.TipoMovimiento)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(m => m.Valor).HasPrecision(18, 2);
        builder.Property(m => m.Saldo).HasPrecision(18, 2);

        // Cuenta is the aggregate root: Movimiento belongs to one account.
        builder.HasOne<Cuenta>()
            .WithMany(c => c.Movimientos)
            .HasForeignKey(m => m.NumeroCuenta)
            .HasConstraintName("FK_Movimientos_Cuentas");
    }
}