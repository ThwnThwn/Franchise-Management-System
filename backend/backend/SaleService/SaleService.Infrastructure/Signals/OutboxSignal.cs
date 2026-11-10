using System.Threading.Channels;
using SalesService.Application.Signals;

namespace SalesService.Infrastructure.Signals;

/// <summary>
/// Triển khai IOutboxSignal sử dụng System.Threading.Channels.
/// Cho phép giao tiếp tín hiệu nội bộ (intra-process) hiệu quả và thread-safe.
/// </summary>
public class OutboxSignal : IOutboxSignal
{
    private readonly Channel<int> _channel;

    public OutboxSignal()
    {
        _channel = Channel.CreateBounded<int>(new BoundedChannelOptions(1)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false
        });
    }

    public void Pulse()
    {
        _channel.Writer.TryWrite(1);
    }

    public async Task WaitForPulseAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _channel.Reader.ReadAsync(cancellationToken);
        }
        catch (ChannelClosedException)
        {
            // Ignore
        }
    }
}