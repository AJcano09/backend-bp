namespace AccountService.Application.DTOs;

/// <summary>Payload to register a deposit or a withdrawal on an account (F2/F3).</summary>
public sealed record RegistrarMovimientoRequest(
    int NumeroCuenta,
    string Tipo,
    decimal Valor);