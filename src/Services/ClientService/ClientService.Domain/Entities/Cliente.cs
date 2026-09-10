using ClientService.Domain.ValueObjects;

namespace ClientService.Domain.Entities;

/// <summary>
/// Bank client. Inherits from <see cref="Persona"/> and adds the access
/// account attributes: password and status.
/// The identity is inherited from <see cref="Persona"/> (Id). ClienteId is
/// the conceptual/API name of that identity (see ClienteConfiguration and the
/// response DTO); EF Core TPT maps it to the Id column in both tables.
/// </summary>
public sealed class Cliente : Persona
{
    public string Contrasena { get; private set; } = null!;
    public bool Estado { get; private set; }

    /// <summary>Required by EF Core to materialize entities.</summary>
    private Cliente() { }

    public Cliente(
        string nombre,
        string genero,
        int edad,
        string identificacion,
        string direccion,
        Telefono telefono,
        string contrasena,
        bool estado)
        : base(nombre, genero, edad, identificacion, direccion, telefono)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contrasena);

        Contrasena = contrasena;
        Estado = estado;
    }

    public void ChangePassword(string nuevaContrasena)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nuevaContrasena);
        Contrasena = nuevaContrasena;
    }

    public void Activate() => Estado = true;

    public void Deactivate() => Estado = false;
}