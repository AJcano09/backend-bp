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

    void Update(Cuenta cuenta);

    /// <summary>True when the client is already mirrored in ClientesLectura (read model).</summary>
    Task<bool> ClienteExisteAsync(int clienteId, CancellationToken cancellationToken = default);
}