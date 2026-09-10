using ClientService.Domain.Entities;
using ClientService.Domain.ValueObjects;
using Xunit;

namespace ClientService.Domain.Tests;

public class ClienteTests
{
    private static Cliente CrearClienteValido(
        string nombre = "Jose Lema",
        string genero = "Masculino",
        int edad = 35,
        string identificacion = "1710034065",
        string direccion = "Otavalo sn y principal",
        string telefono = "+593987654321",
        string contrasena = "1234",
        bool estado = true)
        => new(nombre, genero, edad, identificacion, direccion, Telefono.Create(telefono), contrasena, estado);

    [Fact]
    public void Constructor_ConDatosValidos_AsignaTodasLasPropiedades()
    {
        var cliente = CrearClienteValido();

        Assert.Equal("Jose Lema", cliente.Nombre);
        Assert.Equal("Masculino", cliente.Genero);
        Assert.Equal(35, cliente.Edad);
        Assert.Equal("1710034065", cliente.Identificacion);
        Assert.Equal("Otavalo sn y principal", cliente.Direccion);
        Assert.Equal("+593987654321", cliente.Telefono.Value);
        Assert.Equal("1234", cliente.Contrasena);
        Assert.True(cliente.Estado);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ConNombreEnBlanco_LanzaArgumentException(string nombreInvalido)
    {
        Assert.Throws<ArgumentException>(() => CrearClienteValido(nombre: nombreInvalido));
    }

    [Fact]
    public void Constructor_ConNombreNull_LanzaArgumentNullException()
    {
        // ThrowIfNullOrWhiteSpace(null) lanza ArgumentNullException (no ArgumentException),
        // porque ya sabe que es el argumento el que falta, no su contenido.
        var excepcion = Assert.Throws<ArgumentNullException>(() => CrearClienteValido(nombre: null!));
        Assert.Equal("nombre", excepcion.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(131)]
    [InlineData(200)]
    public void Constructor_ConEdadFueraDeRango_LanzaArgumentOutOfRangeException(int edadInvalida)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CrearClienteValido(edad: edadInvalida));
    }

    [Fact]
    public void Constructor_ConContrasenaVacia_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => CrearClienteValido(contrasena: ""));
    }

    [Fact]
    public void ChangePassword_ConContrasenaValida_ActualizaLaContrasena()
    {
        var cliente = CrearClienteValido();

        cliente.ChangePassword("nuevaClave123");

        Assert.Equal("nuevaClave123", cliente.Contrasena);
    }

    [Fact]
    public void ChangePassword_ConContrasenaVacia_LanzaArgumentExceptionYNoModificaElEstadoPrevio()
    {
        var cliente = CrearClienteValido(contrasena: "original");

        Assert.Throws<ArgumentException>(() => cliente.ChangePassword(""));
        Assert.Equal("original", cliente.Contrasena); // la excepción no debe dejar el objeto a medio modificar
    }

    [Fact]
    public void Deactivate_CambiaEstadoAFalse()
    {
        var cliente = CrearClienteValido(estado: true);

        cliente.Deactivate();

        Assert.False(cliente.Estado);
    }

    [Fact]
    public void Activate_CambiaEstadoATrue()
    {
        var cliente = CrearClienteValido(estado: false);

        cliente.Activate();

        Assert.True(cliente.Estado);
    }

    [Fact]
    public void UpdateBasicData_ConDatosValidos_ActualizaLosCamposHeredadosDePersona()
    {
        var cliente = CrearClienteValido();

        cliente.UpdateBasicData(
            "Jose Lema Actualizado", "Masculino", 36, "1710034065",
            "Nueva dirección", Telefono.Create("+593999999999"));

        Assert.Equal("Jose Lema Actualizado", cliente.Nombre);
        Assert.Equal(36, cliente.Edad);
        Assert.Equal("Nueva dirección", cliente.Direccion);
        Assert.Equal("+593999999999", cliente.Telefono.Value);
    }
}