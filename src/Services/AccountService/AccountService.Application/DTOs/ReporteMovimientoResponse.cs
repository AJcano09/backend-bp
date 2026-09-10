using System.Text.Json.Serialization;

namespace AccountService.Application.DTOs;

/// <summary>
/// F4: una fila del estado de cuenta — un movimiento con el contexto de su
/// cuenta y su cliente. Los nombres de propiedad JSON llevan espacio porque
/// así los especifica el enunciado del ejercicio.
/// </summary>
public sealed record ReporteMovimientoResponse(
    [property: JsonPropertyName("Fecha")] DateTime Fecha,
    [property: JsonPropertyName("Cliente")] string Cliente,
    [property: JsonPropertyName("Numero Cuenta")] int NumeroCuenta,
    [property: JsonPropertyName("Tipo")] string Tipo,
    [property: JsonPropertyName("Saldo Inicial")] decimal SaldoInicial,
    [property: JsonPropertyName("Estado")] bool Estado,
    [property: JsonPropertyName("Movimiento")] decimal Movimiento,
    [property: JsonPropertyName("Saldo Disponible")] decimal SaldoDisponible
);