namespace ClientService.Application.Dtos;

/// <summary>Create client request (F1 - Create use case).</summary>
public sealed record CreateClienteRequest(
    string Nombre,
    string Genero,
    int Edad,
    string Identificacion,
    string Direccion,
    string Telefono,
    string Contrasena,
    bool Estado = true);