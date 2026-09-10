using AccountService.Application.Exceptions;
using AccountService.Application.Ports;
using AccountService.Domain.Entities;
using AccountService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core adapter of ICuentaRepository. Cuenta is always loaded with its
/// movements: the aggregate needs the full ledger to compute SaldoDisponible
/// and validate F3 at registration time.
/// </summary>
public sealed class CuentaRepository : ICuentaRepository
{
    private readonly AccountDbContext _dbContext;

    public CuentaRepository(AccountDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Cuenta?> GetByNumeroCuentaAsync(int numeroCuenta, CancellationToken cancellationToken = default)
        => await _dbContext.Cuentas
            .Include(c => c.Movimientos)
            .FirstOrDefaultAsync(c => c.NumeroCuenta == numeroCuenta, cancellationToken);

    public async Task<IReadOnlyList<Cuenta>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Cuentas
            .Include(c => c.Movimientos)
            .AsNoTracking()
            .OrderBy(c => c.NumeroCuenta)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Cuenta cuenta, CancellationToken cancellationToken = default)
    {
        await _dbContext.Cuentas.AddAsync(cuenta, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Cuenta cuenta, CancellationToken cancellationToken = default)
    {
        // Entities in the graph with a default key (e.g. a brand new
        // Movimiento) are marked Added by EF, so the INSERT happens here with
        // identity fix-up; the rest are updated in place.
        _dbContext.Cuentas.Update(cuenta);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> ClienteExisteAsync(int clienteId, CancellationToken cancellationToken = default)
        => _dbContext.ClientesLectura.AnyAsync(c => c.ClienteId == clienteId, cancellationToken);

    public async Task<IReadOnlyList<Cuenta>> GetByClienteIdConMovimientosEnRangoAsync(
        int clienteId,
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default)
        => await _dbContext.Cuentas
            .Where(c => c.ClienteId == clienteId)
            .Include(c => c.Movimientos.Where(m => m.Fecha >= desde && m.Fecha <= hasta))
            .AsNoTracking()
            .OrderBy(c => c.NumeroCuenta)
            .ToListAsync(cancellationToken);

    public async Task<string> GetNombreClienteAsync(int clienteId, CancellationToken cancellationToken = default)
        => (await _dbContext.ClientesLectura
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ClienteId == clienteId, cancellationToken))
            ?.Nombre
            ?? throw new ClienteNotFoundException(clienteId);
}