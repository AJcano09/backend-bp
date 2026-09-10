namespace AccountService.Domain.Exceptions;

/// <summary>
/// F3: the operation has no available balance. The message is the exact
/// user-facing text required by the statement ("Saldo no disponible").
/// </summary>
public sealed class SaldoInsuficienteException : Exception
{
    public SaldoInsuficienteException()
        : base("Saldo no disponible")
    {
    }
}