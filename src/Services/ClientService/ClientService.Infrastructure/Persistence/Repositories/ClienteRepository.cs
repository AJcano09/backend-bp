using ClientService.Application.Ports;
using ClientService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClientService.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core adapter of the IClienteRepository port.
/// Implements the real persistence without polluting Application or Domain.
/// Soft-delete filtering is explicit here (TPT does not support query filters
/// on derived entity types).
/// </summary>
public sealed class ClienteRepository : IClienteRepository
{
    private readonly ClientDbContext _context;

    public ClienteRepository(ClientDbContext context)
    {
        _context = context;
    }

    public async Task<Cliente?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await ActiveClients()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<Cliente?> GetByIdentificacionAsync(string identificacion, CancellationToken cancellationToken = default)
        => await ActiveClients()
            .FirstOrDefaultAsync(c => c.Identificacion == identificacion, cancellationToken);

    public async Task<IReadOnlyList<Cliente>> GetAllAsync(CancellationToken cancellationToken = default)
        => await ActiveClients()
            .AsNoTracking()
            .OrderBy(c => c.Nombre)
            .ToListAsync(cancellationToken);

    public async Task<bool> ExistsByIdentificacionAsync(string identificacion, CancellationToken cancellationToken = default)
        => await ActiveClients()
            .AnyAsync(c => c.Identificacion == identificacion, cancellationToken);

    public async Task AddAsync(Cliente cliente, CancellationToken cancellationToken = default)
    {
        await _context.Clientes.AddAsync(cliente, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Cliente cliente, CancellationToken cancellationToken = default)
    {
        _context.Clientes.Update(cliente);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// DRY: single predicate for soft-delete filtering.
    /// All read operations go through this to guarantee consistent behavior.
    /// </summary>
    private IQueryable<Cliente> ActiveClients()
        => _context.Clientes.Where(c => c.Estado);
}