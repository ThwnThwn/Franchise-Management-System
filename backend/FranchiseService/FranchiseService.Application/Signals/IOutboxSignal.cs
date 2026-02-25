namespace FranchiseService.Application.Signals;

/// <summary>
/// Cung cấp cơ chế báo hiệu (Signaling) để đánh thức OutboxWorker ngay lập tức khi có tin nhắn mới.
/// Cơ chế này hoạt động như một cái "chuông", giúp giảm độ trễ so với việc Polling (quét định kỳ).
/// </summary>
public interface IOutboxSignal
{
    /// <summary>
    /// Gửi tín hiệu (bấm chuông) để đánh thức worker đang chờ.
    /// Gọi hàm này ngay sau khi thêm tin nhắn vào Outbox.
    /// </summary>
    void Pulse();

    /// <summary>
    /// Chờ tín hiệu một cách bất đồng bộ.
    /// Hàm này giúp worker chờ đợi hiệu quả mà không tốn tài nguyên CPU (không busy-waiting).
    /// </summary>
    Task WaitForPulseAsync(CancellationToken cancellationToken);
}
