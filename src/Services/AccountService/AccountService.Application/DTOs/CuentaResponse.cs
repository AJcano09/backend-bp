namespace AccountService.Application.DTOs;

/// <summary>
/// Account read model returned by the API. SaldoDisponible is always derived
/// from the immutable ledger (SaldoInicial + movements), never stored.
/// </summary>
public sealed record CuentaResponse(
    int NumeroCuenta,
    string TipoCuenta,
    decimal SaldoInicial,
    decimal SaldoDisponible,
    bool Estado,
    int ClienteId)
{
    public static CuentaResponse FromDomain(Domain.Entities.Cuenta cuenta) =>
        new(
            cuenta.NumeroCuenta,
            cuenta.TipoCuenta.ToString(),
            cuenta.SaldoInicial,
            cuenta.SaldoDisponible,
            cuenta.Estado,
            cuenta.ClienteId);
}