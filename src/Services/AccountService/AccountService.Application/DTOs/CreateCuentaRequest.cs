namespace AccountService.Application.DTOs;

/// <summary>Payload to create an account (spec use case 2/3).</summary>
public sealed record CreateCuentaRequest(
    int NumeroCuenta,
    string Tipo,
    decimal SaldoInicial,
    int ClienteId);