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

    public void Update(Cuenta cuenta)
    {
        // Changed entities are tracked by the context; SaveChanges is issued
        // by the consumer scope (the API pipeline), NOT by the repository.
        _dbContext.Cuentas.Update(cuenta);
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