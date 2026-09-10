using AccountService.Application.DTOs;
using AccountService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AccountService.Api.Controllers;

/// <summary>
/// F4: consolidated account statement by client and date range.
/// GET /api/reportes?clienteId=&amp;desde=&amp;hasta=
///
/// The statement ("/reportes?fecha=rango fechas &amp; cliente" with a single
/// "fecha" parameter) does not specify how to encode a range in one value;
/// this implementation interprets it as two explicit date parameters (desde /
/// hasta, yyyy-MM-dd) because they are easier to validate and document.
/// Returns one JSON row per movement inside the range, with the exact keys of
/// the statement example (including spaces, e.g. "Numero Cuenta").
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class ReportesController : ControllerBase
{
    private readonly ReporteService _reporteService;

    public ReportesController(ReporteService reporteService)
        => _reporteService = reporteService;

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ReporteMovimientoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<ReporteMovimientoResponse>>> GetEstadoCuenta(
        [FromQuery] int clienteId,
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta,
        CancellationToken cancellationToken)
    {
        var reporte = await _reporteService.GenerarEstadoCuentaAsync(clienteId, desde, hasta, cancellationToken);
        return Ok(reporte);
    }
}