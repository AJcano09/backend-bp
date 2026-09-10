namespace ClientService.Application.Ports;

/// <summary>
/// Domain event published after a client change.
/// AccountService consumes it (via RabbitMQ) to keep its ClientesLectura
/// read model in sync and answer the F4 report without reading the partner
/// database.
/// </summary>
public sealed record ClienteChangedEvent(
    string Tipo,
    int ClienteId,
    string Nombre,
    DateTime OcurridoEn)
{
    public const string Created = "cliente.created";
    public const string Updated = "cliente.updated";
    public const string Deleted = "cliente.deleted";
}