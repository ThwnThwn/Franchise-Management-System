using DotNetCore.CAP;
using ProductService.Application.Interfaces;

namespace ProductService.Infrastructure.Messaging;

public class CapEventBus : IIntegrationEventBus
{
    private readonly ICapPublisher _capPublisher;

    public CapEventBus(ICapPublisher capPublisher)
    {
        _capPublisher = capPublisher;
    }

    public async Task PublishAsync<T>(string topic, T @event, CancellationToken ct = default) where T : class
    {
        // CAP sẽ tự động check xem có Transaction nào đang mở ở DbContext trong cùng Scope không
        await _capPublisher.PublishAsync(topic, @event, cancellationToken: ct);
    }
}