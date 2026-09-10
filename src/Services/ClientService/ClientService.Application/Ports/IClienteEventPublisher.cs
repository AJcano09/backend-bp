namespace ClientService.Application.Ports;

/// <summary>
/// Async communication port with the rest of the microservices.
/// Concrete implementation: RabbitMQ (Infrastructure).
/// </summary>
public interface IClienteEventPublisher
{
    Task PublishAsync(ClienteChangedEvent evt, CancellationToken cancellationToken = default);
}