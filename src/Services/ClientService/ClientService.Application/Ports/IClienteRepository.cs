using ClientService.Domain.Entities;

namespace ClientService.Application.Ports;

/// <summary>
/// Outbound port (interface) towards persistence.
/// Application defines the contract; Infrastructure implements it.
/// Note the absence of physical delete: removal means soft delete.
/// </summary>
public interface IClienteRepository
{
    Task<Cliente?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Cliente?> GetByIdentificacionAsync(string identificacion, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Cliente>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<bool> ExistsByIdentificacionAsync(string identificacion, CancellationToken cancellationToken = default);

    Task AddAsync(Cliente cliente, CancellationToken cancellationToken = default);

    /// <summary>Persists an existing client (marks modified + SaveChanges).</summary>
    Task UpdateAsync(Cliente cliente, CancellationToken cancellationToken = default);
}