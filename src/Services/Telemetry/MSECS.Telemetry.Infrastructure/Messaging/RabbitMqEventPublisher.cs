using MSECS.BuildingBlocks.Messaging;
using MSECS.Telemetry.Application.Common.Interfaces;

namespace MSECS.Telemetry.Infrastructure.Messaging;

public class RabbitMqEventPublisher : IEventPublisher
{
    private readonly IEventBus _eventBus;

    public RabbitMqEventPublisher(IEventBus eventBus) => _eventBus = eventBus;

    public void Publish(string routingKey, object payload, string eventType)
    {
        var envelope = new IntegrationEvent(Guid.NewGuid(), eventType, DateTimeOffset.UtcNow, CorrelationId: null, payload);
        _eventBus.Publish(routingKey, envelope);
    }
}
