namespace SharedKernel.Abstractions;

/// <summary>
/// Interface Generic cho Repository Outbox.
/// </summary>
/// <typeparam name="TMessage">Loại Message implement IOutboxMessage.</typeparam>
public interface IOutboxRepository<in TMessage> where TMessage : IOutboxMessage
{
    void Add(TMessage message);
}
