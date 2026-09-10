using ClientService.Domain.ValueObjects;

namespace ClientService.Domain.Entities;

/// <summary>
/// Base entity of the hierarchy. Holds the personal data shared by every
/// bank actor. Abstract because a person has no meaning without a concrete
/// role (Cliente) in this domain.
/// </summary>
public abstract class Persona
{
    public int Id { get; protected set; }

    public string Nombre { get; protected set; } = null!;
    public string Genero { get; protected set; } = null!;
    public int Edad { get; protected set; }
    public string Identificacion { get; protected set; } = null!;
    public string Direccion { get; protected set; } = string.Empty;
    public Telefono Telefono { get; protected set; } = null!;

    /// <summary>Required by EF Core to materialize entities.</summary>
    protected Persona() { }

    protected Persona(
        string nombre,
        string genero,
        int edad,
        string identificacion,
        string direccion,
        Telefono telefono)
        => SetBasicData(nombre, genero, edad, identificacion, direccion, telefono);

    public void UpdateBasicData(
        string nombre,
        string genero,
        int edad,
        string identificacion,
        string direccion,
        Telefono telefono)
        => SetBasicData(nombre, genero, edad, identificacion, direccion, telefono);

    private void SetBasicData(
        string nombre,
        string genero,
        int edad,
        string identificacion,
        string direccion,
        Telefono telefono)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        ArgumentException.ThrowIfNullOrWhiteSpace(genero);
        ArgumentException.ThrowIfNullOrWhiteSpace(identificacion);
        ArgumentNullException.ThrowIfNull(telefono);

        if (edad is <= 0 or > 130)
            throw new ArgumentOutOfRangeException(nameof(edad), "Age must be between 1 and 130 years.");

        Nombre = nombre.Trim();
        Genero = genero.Trim();
        Edad = edad;
        Identificacion = identificacion.Trim();
        Direccion = direccion.Trim();
        Telefono = telefono;
    }
}