using FranchiseService.Application.Signals;
using System.Threading.Channels;

namespace FranchiseService.Infrastructure.Signals;

/// <summary>
/// Triển khai IOutboxSignal sử dụng System.Threading.Channels.
/// Cho phép giao tiếp tín hiệu nội bộ (intra-process) hiệu quả và thread-safe.
/// </summary>
public class OutboxSignal : IOutboxSignal
{
    private readonly Channel<int> _channel;

    public OutboxSignal()
    {
        // Bounded channel với capacity 1 là đủ cho tín hiệu "có hàng".
        // FullMode.DropOldest: Nếu worker chưa kịp xử lý mà có thêm tín hiệu thì cứ ghi đè (vì worker khi tỉnh dậy sẽ quét hết).
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
            // Chờ cho đến khi có item trong channel
            await _channel.Reader.ReadAsync(cancellationToken);
        }
        catch (ChannelClosedException)
        {
            // Kênh bị đóng chủ động, bỏ qua
        }
    }
}
