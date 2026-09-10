using ClientService.Application.Dtos;
using ClientService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClientService.Api.Controllers;

/// <summary>
/// F1 - Cliente CRUD. Routes: /api/clientes
/// PUT carries every editable field (full update); DELETE is a soft delete
/// (Estado = false). Errors are mapped centrally by GlobalExceptionHandler.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class ClientesController : ControllerBase
{
    private readonly ClienteService _clienteService;

    public ClientesController(ClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    /// <summary>Creates a client. Returns 201 + Location + body.</summary>
    [HttpPost]
    [ProducesResponseType<ClienteResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClienteResponse>> Create(
        CreateClienteRequest request,
        CancellationToken cancellationToken)
    {
        var cliente = await _clienteService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = cliente.ClienteId }, cliente);
    }

    /// <summary>Lists all clients.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ClienteResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ClienteResponse>>> GetAll(
        CancellationToken cancellationToken)
        => Ok(await _clienteService.GetAllAsync(cancellationToken));

    /// <summary>Gets one client by id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<ClienteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var cliente = await _clienteService.GetByIdAsync(id, cancellationToken);
        return cliente is null ? NotFound() : Ok(cliente);
    }

    /// <summary>Full update of a client (F1 - Update use case).</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType<ClienteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteResponse>> Update(
        int id,
        UpdateClienteRequest request,
        CancellationToken cancellationToken)
        => Ok(await _clienteService.UpdateAsync(id, request, cancellationToken));

    /// <summary>
    /// Logical delete: the client is deactivated (Estado = false) but the row
    /// stays, preserving the FK with accounts. Banking semantics.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        await _clienteService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}