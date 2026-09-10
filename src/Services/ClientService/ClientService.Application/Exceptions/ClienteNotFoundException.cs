namespace ClientService.Application.Exceptions;

/// <summary>Thrown when a client does not exist or is inactive (soft delete).</summary>
public sealed class ClienteNotFoundException : Exception
{
    public ClienteNotFoundException(int clienteId)
        : base($"The client with id {clienteId} does not exist.")
    {
    }

    public ClienteNotFoundException(string identificacion)
        : base($"The client with identification '{identificacion}' does not exist.")
    {
    }
}