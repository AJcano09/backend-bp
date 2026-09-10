using AccountService.Application.DTOs;
using AccountService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AccountService.Api.Controllers;

/// <summary>
/// Account CRU API (F1): POST /cuentas, GET /cuentas, GET /cuentas/{numeroCuenta},
/// PUT /cuentas/{numeroCuenta}. No physical DELETE: accounts are soft-deleted
/// through Estado (the ledger must survive).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class CuentasController : ControllerBase
{
    private readonly CuentaService _cuentaService;

    public CuentasController(CuentaService cuentaService)
    {
        _cuentaService = cuentaService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CuentaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CuentaResponse>> Create([FromBody] CreateCuentaRequest request, CancellationToken cancellationToken)
    {
        var cuenta = await _cuentaService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetByNumeroCuenta), new { numeroCuenta = cuenta.NumeroCuenta }, CuentaResponse.FromDomain(cuenta));
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CuentaResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CuentaResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var cuentas = await _cuentaService.GetAllAsync(cancellationToken);
        return Ok(cuentas.Select(CuentaResponse.FromDomain));
    }

    [HttpGet("{numeroCuenta:int}")]
    [ProducesResponseType(typeof(CuentaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CuentaResponse>> GetByNumeroCuenta(int numeroCuenta, CancellationToken cancellationToken)
    {
        var cuenta = await _cuentaService.GetByNumeroCuentaAsync(numeroCuenta, cancellationToken);
        return Ok(CuentaResponse.FromDomain(cuenta));
    }

    [HttpPut("{numeroCuenta:int}")]
    [ProducesResponseType(typeof(CuentaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CuentaResponse>> Update(int numeroCuenta, [FromBody] UpdateCuentaRequest request, CancellationToken cancellationToken)
    {
        var cuenta = await _cuentaService.UpdateAsync(numeroCuenta, request, cancellationToken);
        return Ok(CuentaResponse.FromDomain(cuenta));
    }
}