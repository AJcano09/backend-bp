using AccountService.Domain.Exceptions;
using AccountService.Domain.ValueObjects;

namespace AccountService.Domain.Entities;

/// <summary>
/// Single transaction of the account ledger (F2). Immutable: created only by
/// Cuenta.RegistrarMovimiento and never modified or deleted. Saldo is the
/// balance AFTER this movement was applied and is self-computed: the entity
/// refuses to exist in an inconsistent state.
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

    /// <summary>
    /// Creates a movement and computes its post-balance from the previous
    /// balance. Defense in depth: even a bypass of the aggregate cannot create
    /// a movement with a non-positive value or a negative post-balance (F3).
    /// </summary>
    /// <param name="tipoMovimiento">Deposito or Retiro; the sign is owned by the type.</param>
    /// <param name="valor">Positive magnitude of the movement.</param>
    /// <param name="saldoAnterior">Balance before applying this movement.</param>
    /// <param name="numeroCuenta">Account number this movement belongs to.</param>
    internal Movimiento(TipoMovimiento tipoMovimiento, decimal valor, decimal saldoAnterior, int numeroCuenta)
    {
        if (valor == 0)
            throw new ArgumentOutOfRangeException(nameof(valor), "The movement value cannot be zero.");

        // F2: the caller may express the value with either sign; the stored
        // magnitude is normalized to positive and the type owns the sign.
        var magnitud = Math.Abs(valor);

        // UTC wall-clock as a naive timestamp: the storage contract is
        // "timestamp without time zone" (matches the deliverable BaseDatos.sql
        // and MovimientoConfiguration). Npgsql rejects a Kind=UTC DateTime
        // for that column type, so the Kind is stripped after UTC is captured.
        Fecha = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        TipoMovimiento = tipoMovimiento;
        Valor = magnitud;
        NumeroCuenta = numeroCuenta;
        Saldo = saldoAnterior + tipoMovimiento.Signo() * magnitud;

        if (Saldo < 0)
            throw new SaldoInsuficienteException();
    }
}