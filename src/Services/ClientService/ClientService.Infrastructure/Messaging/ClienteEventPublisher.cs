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
/// Best-effort with bounded retry: a transient broker bounce (startup race,
/// connectivity blip) is retried with backoff so the event is not silently
/// lost; a long outage is logged as an error and the read model becomes
/// eventually consistent with later events. The durable queue is declared
/// here too so events published before the consumer connects are queued
/// instead of dropped by the fanout exchange.
/// </summary>
public sealed class ClienteEventPublisher : IClienteEventPublisher, IDisposable
{
    private static readonly TimeSpan[] RetryDelays =
    [
        TimeSpan.FromMilliseconds(500),
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(2),
        TimeSpan.FromSeconds(4),
        TimeSpan.FromSeconds(8),
    ];

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
        Exception? lastException = null;

        foreach (var delay in RetryDelays)
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
                return;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                lastException = ex;
                ResetTransport();
                await Task.Delay(delay, cancellationToken);
            }
        }

        // All retries exhausted: log as error. The use case still succeeds on
        // the client side and the read model catches up with the next event.
        _logger.LogError(lastException, "Failed to publish event {Tipo} (client {ClienteId}) after {Attempts} attempts",
            evt.Tipo, evt.ClienteId, RetryDelays.Length);
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

            // Mirror the consumer's queue declaration (idempotent) so a
            // publish that happens before the consumer connects is queued
            // instead of dropped by the unrouted fanout exchange.
            _channel.QueueDeclare(_options.Queue, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(_options.Queue, _options.Exchange, routingKey: "cliente.*");

            return _channel;
        }
        finally
        {
            _lock.Release();
        }
    }

    private void ResetTransport()
    {
        _lock.Wait();
        try
        {
            try { _channel?.Close(); } catch { /* best-effort */ }
            _channel?.Dispose();
            _channel = null;
            try { _connection?.Dispose(); } catch { /* best-effort */ }
            _connection = null;
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