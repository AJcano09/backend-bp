using AccountService.Domain.ValueObjects;

namespace AccountService.Domain.Entities;

/// <summary>
/// Single transaction of the account ledger (F2). Immutable: created only by
/// Cuenta.RegistrarMovimiento and never modified or deleted. Saldo is the
/// balance AFTER this movement was applied.
/// </summary>
public sealed class Movimiento
{
    public int Id { get; private set; }

    public DateTime Fecha { get; private set; }

    public TipoMovimiento TipoMovimiento { get; private set; }

    /// <summary>
    /// Positive magnitude; the sign is given by the type
    /// (Retiro contributes negatively to the balance).
    /// </summary>
    public decimal Valor { get; private set; }

    /// <summary>Balance after applying this movement.</summary>
    public decimal Saldo { get; private set; }

    public int NumeroCuenta { get; private set; }

    private Movimiento()
    {
        // EF Core materialization.
    }

    internal Movimiento(TipoMovimiento tipoMovimiento, decimal valor, decimal saldo, int numeroCuenta)
    {
        Fecha = DateTime.UtcNow;
        TipoMovimiento = tipoMovimiento;
        Valor = valor;
        Saldo = saldo;
        NumeroCuenta = numeroCuenta;
    }
}