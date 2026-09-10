namespace AccountService.Application.DTOs;

/// <summary>
/// Payload to update an account. The only editable field is the account type:
/// the initial balance is the ledger anchor (F2) and the status is managed by
/// Activate/Deactivate flows.
/// </summary>
public sealed record UpdateCuentaRequest(string Tipo);