using ClientService.Application.Dtos;
using ClientService.Application.Exceptions;
using ClientService.Application.Ports;
using ClientService.Domain.Entities;
using ClientService.Domain.ValueObjects;

namespace ClientService.Application.Services;

/// <summary>
/// Use cases (F1) of the Cliente aggregate.
/// Orchestrates the domain, the persistence (port) and event publishing.
/// Knows nothing about EF Core or HTTP: depends only on interfaces.
/// </summary>
public sealed class ClienteService
{
    private readonly IClienteRepository _repository;
    private readonly IClienteEventPublisher _eventPublisher;

    public ClienteService(IClienteRepository repository, IClienteEventPublisher eventPublisher)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
    }

    public async Task<ClienteResponse> CreateAsync(CreateClienteRequest request, CancellationToken cancellationToken = default)
    {
        if (await _repository.ExistsByIdentificacionAsync(request.Identificacion, cancellationToken))
            throw new InvalidOperationException(
                $"A client with identification '{request.Identificacion}' already exists.");

        var telefono = Telefono.Create(request.Telefono);
        var cliente = new Cliente(
            request.Nombre,
            request.Genero,
            request.Edad,
            request.Identificacion,
            request.Direccion,
            telefono,
            request.Contrasena,
            request.Estado);

        await _repository.AddAsync(cliente, cancellationToken);

        await _eventPublisher.PublishAsync(new ClienteChangedEvent(
            ClienteChangedEvent.Created, cliente.Id, cliente.Nombre, cliente.Estado, DateTime.UtcNow), cancellationToken);

        return ClienteResponse.FromDomain(cliente);
    }

    public async Task<ClienteResponse> UpdateAsync(int id, UpdateClienteRequest request, CancellationToken cancellationToken = default)
    {
        var cliente = await GetActiveOrThrowAsync(id, cancellationToken);

        var telefono = Telefono.Create(request.Telefono);
        cliente.UpdateBasicData(
            request.Nombre, request.Genero, request.Edad,
            request.Identificacion, request.Direccion, telefono);
        cliente.ChangePassword(request.Contrasena);

        if (request.Estado) cliente.Activate();
        else cliente.Deactivate();

        await _repository.UpdateAsync(cliente, cancellationToken);

        await _eventPublisher.PublishAsync(new ClienteChangedEvent(
            ClienteChangedEvent.Updated, cliente.Id, cliente.Nombre, cliente.Estado, DateTime.UtcNow), cancellationToken);

        return ClienteResponse.FromDomain(cliente);
    }

    /// <summary>
    /// Logical delete (soft delete): the client stays with Estado = false.
    /// The row is never physically removed: the FK with accounts is preserved
    /// and "inactive client" is the real banking semantics.
    /// </summary>
    public async Task<ClienteResponse> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var cliente = await GetActiveOrThrowAsync(id, cancellationToken);

        cliente.Deactivate();
        await _repository.UpdateAsync(cliente, cancellationToken);

        await _eventPublisher.PublishAsync(new ClienteChangedEvent(
            ClienteChangedEvent.Deleted, cliente.Id, cliente.Nombre, cliente.Estado, DateTime.UtcNow), cancellationToken);

        return ClienteResponse.FromDomain(cliente);
    }

    public async Task<ClienteResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cliente = await _repository.GetByIdAsync(id, cancellationToken);
        return cliente is null ? null : ClienteResponse.FromDomain(cliente);
    }

    public async Task<IReadOnlyList<ClienteResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var clientes = await _repository.GetAllAsync(cancellationToken);
        return clientes.Select(ClienteResponse.FromDomain).ToList();
    }

    private async Task<Cliente> GetActiveOrThrowAsync(int id, CancellationToken cancellationToken)
        => await _repository.GetByIdAsync(id, cancellationToken)
           ?? throw new ClienteNotFoundException(id);
}