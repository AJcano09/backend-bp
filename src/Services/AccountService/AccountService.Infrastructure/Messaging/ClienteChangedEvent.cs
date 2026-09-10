namespace AccountService.Infrastructure.Messaging;

/// <summary>
/// Client changed event received from the 'clientes' topic exchange
/// (published by ClientService). Keeps the ClientesLectura read model in sync
/// so the account service answers F1/F4 without reading the partner database
/// (async communication between microservices).
/// </summary>
public sealed record ClienteChangedEvent(
    string Tipo,
    int ClienteId,
    string Nombre,
    bool Estado,
    DateTime OcurridoEn)
{
    public const string Created = "cliente.created";
    public const string Updated = "cliente.updated";
    public const string Deleted = "cliente.deleted";
}