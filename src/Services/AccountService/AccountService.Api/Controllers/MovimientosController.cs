using AccountService.Application.DTOs;
using AccountService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AccountService.Api.Controllers;

/// <summary>
/// Movements CRU API (F1, F2, F3): POST /movimientos registers a deposit or
/// a withdrawal; GET /movimientos?cuenta= lists the ledger of an account.
/// No update/delete: movimientos are immutable.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class MovimientosController : ControllerBase
{
    private readonly CuentaService _cuentaService;

    public MovimientosController(CuentaService cuentaService)
    {
        _cuentaService = cuentaService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(MovimientoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovimientoResponse>> Create([FromBody] RegistrarMovimientoRequest request, CancellationToken cancellationToken)
    {
        var cuenta = await _cuentaService.RegistrarMovimientoAsync(request, cancellationToken);
        var movimiento = cuenta.Movimientos.Last();
        return CreatedAtAction(nameof(GetByCuenta), new { cuenta = cuenta.NumeroCuenta }, MovimientoResponse.FromDomain(movimiento));
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MovimientoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<MovimientoResponse>>> GetByCuenta([FromQuery] int cuenta, CancellationToken cancellationToken)
    {
        var movimientos = await _cuentaService.GetMovimientosAsync(cuenta, cancellationToken);
        return Ok(movimientos.Select(MovimientoResponse.FromDomain));
    }
}