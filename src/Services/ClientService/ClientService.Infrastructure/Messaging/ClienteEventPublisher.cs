using System.Text.Json;
using ClientService.Application.Ports;
using ClientService.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace ClientService.Infrastructure.Messaging;

/// <summary>
/// RabbitMQ adapter of the IClienteEventPublisher port.
/// Publishes to the 'clientes' topic exchange with routing key = event type.
/// Best-effort: a broker failure must not take the use case down; it is logged.
/// The connection is created lazily on the first publish.
/// </summary>
public sealed class ClienteEventPublisher : IClienteEventPublisher, IDisposable
{
    private readonly ConnectionFactory _connectionFactory;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<ClienteEventPublisher> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private IConnection? _connection;
    private IModel? _channel;

    public ClienteEventPublisher(
        ConnectionFactory connectionFactory,
        IOptions<RabbitMqOptions> options,
        ILogger<ClienteEventPublisher> logger)
    {
        _connectionFactory = connectionFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task PublishAsync(ClienteChangedEvent evt, CancellationToken cancellationToken = default)
    {
        try
        {
            var body = JsonSerializer.SerializeToUtf8Bytes(evt);
            var channel = await GetChannelAsync(cancellationToken);
            channel.BasicPublish(
                exchange: _options.Exchange,
                routingKey: evt.Tipo,
                basicProperties: null,
                body: body);

            _logger.LogInformation("Event published {Tipo} -> client {ClienteId}", evt.Tipo, evt.ClienteId);
        }
        catch (Exception ex)
        {
            // Eventual consistency: the read model syncs with the next event;
            // a down broker must not break the client CRUD.
            _logger.LogWarning(ex, "Failed to publish event {Tipo} (client {ClienteId})",
                evt.Tipo, evt.ClienteId);
        }
    }

    private async Task<IModel> GetChannelAsync(CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_channel is { IsOpen: true })
                return _channel;

            _connection ??= _connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(_options.Exchange, ExchangeType.Topic, durable: true);
            return _channel;
        }
        finally
        {
            _lock.Release();
        }
    }

    public void Dispose()
    {
        _lock.Wait();
        try
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Dispose();
        }
        finally
        {
            _lock.Release();
            _lock.Dispose();
        }

        GC.SuppressFinalize(this);
    }
}