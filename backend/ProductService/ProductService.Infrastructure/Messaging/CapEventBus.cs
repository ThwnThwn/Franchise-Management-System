using DotNetCore.CAP;
using ProductService.Application.Interfaces;
using ProductService.Application.Repositories;
using ProductService.Infrastructure.Repositories;

namespace ProductService.Infrastructure.Messaging;

public class CapEventBus(ICapPublisher capPublisher, IProductUow uow) : IIntegrationEventBus
{
    private readonly ICapPublisher _capPublisher = capPublisher;
    private readonly IProductUow _uow = uow;

    public async Task PublishAsync<T>(string topic, T @event, CancellationToken ct = default) where T : class
    {
        // Nếu Publisher bị mất bối cảnh (AsyncLocal loss)
        // "mượn" lại hàng chính chủ từ Wrapper trong UoW để nhét lại vào Publisher
        if (_capPublisher.Transaction == null && _uow.IntegrationTransaction is CapTransactionWrapper wrapper)
        {
            _capPublisher.Transaction = wrapper.InternalTransaction;
        }

        await _capPublisher.PublishAsync(topic, @event, cancellationToken: ct);
    }
}