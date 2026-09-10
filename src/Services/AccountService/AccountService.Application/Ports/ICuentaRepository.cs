using AccountService.Domain.Entities;

namespace AccountService.Application.Ports;

/// <summary>
/// Persistence port for the Cuenta aggregate. The repository always loads
/// Cuenta with its movements so the domain can compute SaldoDisponible and
/// validate F3 against the real ledger.
/// </summary>
public interface ICuentaRepository
{
    Task<Cuenta?> GetByNumeroCuentaAsync(int numeroCuenta, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Cuenta>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(Cuenta cuenta, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to an existing Cuenta, including newly registered
    /// movements (the aggregate is always loaded with its ledger). The
    /// repository owns the SaveChanges: there is no external unit of work in
    /// the API pipeline, so relying on one would silently drop writes.
    /// </summary>
    Task UpdateAsync(Cuenta cuenta, CancellationToken cancellationToken = default);

    /// <summary>True when the client is already mirrored in ClientesLectura (read model).</summary>
    Task<bool> ClienteExisteAsync(int clienteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// F4 read-only projection: accounts of one client with ONLY the movements
    /// inside [desde, hasta]. The date filter travels to SQL (filtered Include),
    /// never applied in memory over a fully-loaded ledger.
    /// </summary>
    Task<IReadOnlyList<Cuenta>> GetByClienteIdConMovimientosEnRangoAsync(
        int clienteId,
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default);

    /// <summary>Client display name from the read model (F4 report).</summary>
    Task<string> GetNombreClienteAsync(int clienteId, CancellationToken cancellationToken = default);
}