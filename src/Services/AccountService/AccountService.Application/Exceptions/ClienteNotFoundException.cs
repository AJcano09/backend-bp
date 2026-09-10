namespace AccountService.Application.Exceptions;

/// <summary>
/// The requested client does not exist in the ClientesLectura read model.
/// The API translates it to 404.
/// </summary>
public sealed class ClienteNotFoundException : Exception
{
    public ClienteNotFoundException(int clienteId)
        : base($"The client {clienteId} does not exist in the account service (ClientesLectura).")
    {
        ClienteId = clienteId;
    }

    public int ClienteId { get; }
}