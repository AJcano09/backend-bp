namespace ClientService.Application.Dtos;

/// <summary>Update client request (F1 - Update use case).</summary>
public sealed record UpdateClienteRequest(
    string Nombre,
    string Genero,
    int Edad,
    string Identificacion,
    string Direccion,
    string Telefono,
    string Contrasena,
    bool Estado);