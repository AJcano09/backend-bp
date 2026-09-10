using ClientService.Domain.Entities;

namespace ClientService.Application.Dtos;

/// <summary>
/// Client response. NEVER exposes the password.
/// </summary>
public sealed record ClienteResponse(
    int ClienteId,
    string Nombre,
    string Genero,
    int Edad,
    string Identificacion,
    string Direccion,
    string Telefono,
    bool Estado)
{
    public static ClienteResponse FromDomain(Cliente cliente) => new(
        cliente.Id,
        cliente.Nombre,
        cliente.Genero,
        cliente.Edad,
        cliente.Identificacion,
        cliente.Direccion,
        cliente.Telefono.Value,
        cliente.Estado);
}