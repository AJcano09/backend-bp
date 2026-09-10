using AccountService.Domain.ValueObjects;

namespace AccountService.Domain.Entities;

/// <summary>
/// Bank account (F1 - CRU: create, read, update; no physical delete).
/// Belongs to a client tracked in ClientesLectura (read model). The initial
/// balance is set once at creation; the available balance is derived from the
/// immutable movements ledger (SaldoInicial + sum of movements — F2).
/// </summary>
public sealed class Cuenta
{
    public int NumeroCuenta { get; private set; }

    public TipoCuenta TipoCuenta { get; private set; }

    public decimal SaldoInicial { get; private set; }

    public bool Estado { get; private set; }

    public int ClienteId { get; private set; }

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
}