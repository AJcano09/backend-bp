using System.Text;
using System.Text.Json;
using AccountService.Infrastructure.Options;
using AccountService.Infrastructure.Persistence;
using AccountService.Infrastructure.Persistence.ReadModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AccountService.Infrastructure.Messaging;

/// <summary>
/// RabbitMQ consumer of the 'clientes' topic exchange. Binds a durable queue
/// ('account.clientes') to the exchange (created by the publisher side) and
/// upserts the ClientesLectura read model on every client change.
///
/// Resilience: the host retries with backoff while the broker is down, and
/// delivery is acknowledged only after the read model is persisted, so no
/// event is lost in the happy path. Each delivered message gets its OWN
/// DI scope/DbContext: RabbitMQ delivers with prefetch > 1 and a shared
/// context would throw NpgsqlOperationInProgressException ("command already
/// in progress") under concurrent delivery, which previously lost events.
/// This is the async communication channel required by the statement.
/// </summary>
public sealed class ClienteEventConsumer : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<RabbitMqOptions> _options;
    private readonly ILogger<ClienteEventConsumer> _logger;

    private IConnection? _connection;
    private IModel? _channel;

    public ClienteEventConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> options,
        ILogger<ClienteEventConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var eventingConsumer = Connect();
                await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Graceful shutdown.
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "RabbitMQ consumer unavailable ({Host}); retrying in 2s.",
                    _options.Value.Host);
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }

    private EventingBasicConsumer Connect()
    {
        var options = _options.Value;
        var factory = new ConnectionFactory { HostName = options.Host, DispatchConsumersAsync = false };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // The topic exchange is declared on the publisher side too; declaring
        // here with the same arguments is idempotent and makes the consumer
        // self-sufficient when the publisher never ran (e.g. first boot).
        _channel.ExchangeDeclare(options.Exchange, ExchangeType.Topic, durable: true);
        _channel.QueueDeclare(
            queue: options.Queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
        _channel.QueueBind(options.Queue, options.Exchange, "cliente.*");

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (_, args) => await HandleMessageAsync(args);
        _channel.BasicQos(prefetchSize: 0, prefetchCount: 10, global: false);
        _channel.BasicConsume(queue: options.Queue, autoAck: false, consumer: consumer);

        _logger.LogInformation("RabbitMQ consumer listening on queue {Queue} (exchange {Exchange}).",
            options.Queue, options.Exchange);

        return consumer;
    }

    private async Task HandleMessageAsync(BasicDeliverEventArgs args)
    {
        // One DI scope per delivered message. RabbitMQ invokes this handler
        // concurrently (BasicQos prefetchCount: 10) and a DbContext is not
        // thread-safe: a shared context made a second SELECT collide with the
        // first ("A command is already in progress") and the poison-path ack
        // then silently dropped the event from the read model.
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AccountDbContext>();
        var routingKey = args.RoutingKey;

        try
        {
            var evt = JsonSerializer.Deserialize<ClienteChangedEvent>(args.Body.Span, JsonOptions)
                ?? throw new JsonException("Null event payload.");

            var existente = await dbContext.ClientesLectura.FirstOrDefaultAsync(c => c.ClienteId == evt.ClienteId);

            switch (evt.Tipo)
            {
                case ClienteChangedEvent.Created:
                case ClienteChangedEvent.Updated:
                    if (existente is null)
                    {
                        dbContext.ClientesLectura.Add(new ClientesLectura(evt.ClienteId, evt.Nombre, evt.Estado));
                        _logger.LogInformation("Read model created for client {ClienteId} (Estado={Estado}).",
                            evt.ClienteId, evt.Estado);
                    }
                    else
                    {
                        if (existente.Nombre != evt.Nombre)
                            existente.UpdateNombre(evt.Nombre);
                        if (existente.Estado != evt.Estado)
                            existente.UpdateEstado(evt.Estado);
                    }
                    break;

                case ClienteChangedEvent.Deleted:
                    // Soft-deleted clients keep their accounts and history,
                    // but the row must be marked inactive so the account
                    // service rejects new accounts for them (F1 banking rule).
                    if (existente is not null && existente.Estado)
                    {
                        existente.UpdateEstado(false);
                        _logger.LogInformation("Read model deactivated for client {ClienteId}.", evt.ClienteId);
                    }
                    break;

                default:
                    _logger.LogWarning("Unknown routing key {RoutingKey}; message acknowledged.", routingKey);
                    break;
            }

            await dbContext.SaveChangesAsync();
            _channel?.BasicAck(args.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            // Poison message: ack so the queue does not block; the failure is
            // logged for observability (F3-style resilience, not retry loops).
            _logger.LogError(ex, "Failed to apply event {RoutingKey} for client. Message acknowledged and skipped.",
                routingKey);
            _channel?.BasicAck(args.DeliveryTag, multiple: false);
        }
    }

    public override void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}