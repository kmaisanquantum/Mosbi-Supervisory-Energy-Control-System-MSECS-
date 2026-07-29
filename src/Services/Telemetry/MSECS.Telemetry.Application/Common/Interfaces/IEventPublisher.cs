namespace MSECS.Telemetry.Application.Common.Interfaces;

/// <summary>Thin seam over MSECS.BuildingBlocks.Messaging.IEventBus so Application handlers
/// don't take a direct dependency on RabbitMQ.Client types.</summary>
public interface IEventPublisher
{
    void Publish(string routingKey, object payload, string eventType);
}
