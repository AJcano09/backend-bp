namespace AccountService.Infrastructure.Persistence.ReadModels;

/// <summary>
/// Client read model OWNED by AccountService (bank_accounts). ClienteId is
/// the primary key and Cuenta references it via FK: a bank account cannot
/// exist for a client this service does not know (F1).
/// Kept in sync by consuming ClientService events (cliente.created / updated /
/// deleted) through RabbitMQ.
/// </summary>
public sealed class ClientesLectura
{
    public int ClienteId { get; set; }

    public string Nombre { get; set; } = null!;

    /// <summary>
    /// Mirror of the client's Estado: a deactivated client (soft deleted)
    /// must not own new accounts (F1 banking rule), and F4 reports still show
    /// the name of historical clients.
    /// </summary>
    public bool Estado { get; set; }

    private ClientesLectura()
    {
        // EF Core materialization.
    }

    public ClientesLectura(int clienteId, string nombre, bool estado)
    {
        if (clienteId <= 0)
            throw new ArgumentException("The client id must be a positive integer.", nameof(clienteId));
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("The client name cannot be empty.", nameof(nombre));

        ClienteId = clienteId;
        Nombre = nombre.Trim();
        Estado = estado;
    }

    public void UpdateNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("The client name cannot be empty.", nameof(nombre));

        Nombre = nombre.Trim();
    }

    public void UpdateEstado(bool estado) => Estado = estado;
}