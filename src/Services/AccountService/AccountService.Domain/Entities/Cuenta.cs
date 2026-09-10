using AccountService.Domain.Exceptions;
using AccountService.Domain.ValueObjects;

namespace AccountService.Domain.Entities;

/// <summary>
/// Bank account (F1 - CRU: create, read, update; no physical delete).
/// Aggregate root of the movements ledger (F2): it owns its Movimientos.
/// Belongs to a client tracked in ClientesLectura (read model). The initial
/// balance is set once at creation; the available balance is derived from the
/// immutable ledger (SaldoInicial + sum of movements).
/// </summary>
public sealed class Cuenta
{
    private readonly List<Movimiento> _movimientos = new();

    public int NumeroCuenta { get; private set; }

    public TipoCuenta TipoCuenta { get; private set; }

    public decimal SaldoInicial { get; private set; }

    public bool Estado { get; private set; }

    public int ClienteId { get; private set; }

    /// <summary>Immutable ledger of this account (F2).</summary>
    public IReadOnlyCollection<Movimiento> Movimientos => _movimientos;

    /// <summary>Current balance: initial balance plus all registered movements (withdrawal sign applied).</summary>
    public decimal SaldoDisponible => SaldoInicial + _movimientos.Sum(m => m.TipoMovimiento.Signo() * m.Valor);

    private Cuenta()
    {
        // EF Core materialization.
    }

    public Cuenta(int numeroCuenta, TipoCuenta tipoCuenta, decimal saldoInicial, int clienteId)
    {
        if (numeroCuenta <= 0)
            throw new ArgumentException("The account number must be a positive integer.", nameof(numeroCuenta));
        if (saldoInicial < 0)
            throw new ArgumentException("The initial balance cannot be negative.", nameof(saldoInicial));

        NumeroCuenta = numeroCuenta;
        TipoCuenta = tipoCuenta;
        SaldoInicial = saldoInicial;
        Estado = true;
        ClienteId = clienteId;
    }

    /// <summary>
    /// Updates the editable fields of the account. The initial balance is not
    /// editable: it is the anchor of the ledger and every change after
    /// creation flows through a movement (F2).
    /// </summary>
    public void Update(TipoCuenta tipoCuenta)
    {
        TipoCuenta = tipoCuenta;
    }

    public void Activate() => Estado = true;

    public void Deactivate() => Estado = false;

    /// <summary>
    /// F2/F3: registers a movement on the ledger. Deposits increase the
    /// balance; withdrawals decrease it. If the operation would leave the
    /// balance negative it throws <see cref="SaldoInsuficienteException"/>
    /// (F3, message "Saldo no disponible") and nothing is recorded: the
    /// movement plus its post-balance are one atomic business rule.
    /// </summary>
    public Movimiento RegistrarMovimiento(TipoMovimiento tipoMovimiento, decimal valor)
    {
        if (valor == 0)
            throw new ArgumentException("The movement value cannot be zero.", nameof(valor));
        if (!Estado)
            throw new InvalidOperationException("The account is inactive and cannot register movements.");

        // F2: the API accepts positive or negative values (a withdrawal may be
        // sent as -575). The stored magnitude is always positive; the sign is
        // owned by the movement type (Deposito +1, Retiro -1) and applied
        // exactly once when computing the post-balance.
        var magnitud = Math.Abs(valor);

        // The post-balance is computed by Movimiento itself; Cuenta only
        // provides the previous balance. If the withdrawal exceeds the
        // available balance, Movimiento throws SaldoInsuficienteException
        // (F3, "Saldo no disponible") BEFORE the movement is added: atomic.
        var movimiento = new Movimiento(tipoMovimiento, magnitud, SaldoDisponible, NumeroCuenta);
        _movimientos.Add(movimiento);
        return movimiento;
    }
}