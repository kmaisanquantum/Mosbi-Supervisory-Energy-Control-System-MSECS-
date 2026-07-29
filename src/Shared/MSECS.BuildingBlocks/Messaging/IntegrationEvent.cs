namespace MSECS.BuildingBlocks.Messaging;

/// <summary>
/// Envelope for events published on the RabbitMQ topic exchange. RoutingKey follows the
/// convention "{service}.{aggregate}.{eventName}", e.g. "telemetry.reading.ingested".
/// </summary>
public record IntegrationEvent(
    Guid EventId,
    string EventType,
    DateTimeOffset OccurredOnUtc,
    string? CorrelationId,
    object Payload);

public interface IEventBus
{
    void Publish(string routingKey, IntegrationEvent @event);
}

public interface IEventSubscriber
{
    void Subscribe(string queueName, IEnumerable<string> bindingKeys, Func<string, string, Task> onMessage);
}
