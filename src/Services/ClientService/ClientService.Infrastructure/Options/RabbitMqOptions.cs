namespace ClientService.Infrastructure.Options;

/// <summary>Messaging broker (RabbitMQ) connection options.</summary>
public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMQ";

    public string Host { get; set; } = "localhost";
    public string User { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string Exchange { get; set; } = "clientes";
    public string Queue { get; set; } = "account.clientes";
}