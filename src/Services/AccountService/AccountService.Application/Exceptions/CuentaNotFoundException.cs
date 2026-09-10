namespace AccountService.Application.Exceptions;

/// <summary>
/// The requested account does not exist. The API translates it to 404.
/// </summary>
public sealed class CuentaNotFoundException : Exception
{
    public CuentaNotFoundException(int numeroCuenta)
        : base($"The account {numeroCuenta} does not exist.")
    {
        NumeroCuenta = numeroCuenta;
    }

    public int NumeroCuenta { get; }
}