using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Serilog;

namespace MSECS.BuildingBlocks.Messaging;

/// <summary>
/// Thin wrapper over RabbitMQ.Client publishing to a single durable topic exchange
/// ("msecs.events" by default) shared by all microservices. Each service declares
/// its own durable queue and binds the routing keys it cares about.
/// </summary>
public sealed class RabbitMqEventBus : IEventBus, IEventSubscriber, IDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqEventBus(IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;

        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost,
            DispatchConsumersAsync = true,
            AutomaticRecoveryEnabled = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(_options.Exchange, ExchangeType.Topic, durable: true);
    }

    public void Publish(string routingKey, IntegrationEvent @event)
    {
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event));

        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.MessageId = @event.EventId.ToString();
        properties.Type = @event.EventType;
        properties.CorrelationId = @event.CorrelationId;

        _channel.BasicPublish(_options.Exchange, routingKey, properties, body);
        Log.Debug("Published event {EventType} with routing key {RoutingKey}", @event.EventType, routingKey);
    }

    public void Subscribe(string queueName, IEnumerable<string> bindingKeys, Func<string, string, Task> onMessage)
    {
        _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);

        foreach (var key in bindingKeys)
        {
            _channel.QueueBind(queueName, _options.Exchange, key);
        }

        var consumer = new RabbitMQ.Client.Events.AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) =>
        {
            var routingKey = ea.RoutingKey;
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());
            try
            {
                await onMessage(routingKey, message);
                _channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to process message with routing key {RoutingKey}", routingKey);
                _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        _channel.BasicConsume(queueName, autoAck: false, consumer);
    }

    public void Dispose()
    {
        _channel.Close();
        _connection.Close();
    }
}

public static class RabbitMqExtensions
{
    public static IServiceCollection AddMsecsRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddSingleton<RabbitMqEventBus>();
        services.AddSingleton<IEventBus>(sp => sp.GetRequiredService<RabbitMqEventBus>());
        services.AddSingleton<IEventSubscriber>(sp => sp.GetRequiredService<RabbitMqEventBus>());
        return services;
    }
}
