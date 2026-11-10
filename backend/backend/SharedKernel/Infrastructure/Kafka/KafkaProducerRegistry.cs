namespace SharedKernel.Infrastructure.Kafka;

/// <summary>
/// Registry lưu trữ cấu hình mapping giữa Event Type và Topic cho chiều Producer (Gửi đi).
/// Hữu ích cho pattern Outbox hoặc các worker cần gửi tin dynamic.
/// </summary>
public class KafkaProducerRegistry
{
    private readonly Dictionary<string, (Type EventType, string Topic)> _typeMappings = [];

    /// <summary>
    /// Đăng ký mapping từ Event Type sang Topic.
    /// </summary>
    /// <typeparam name="TEvent">Loại Event.</typeparam>
    /// <param name="topic">Topic đích.</param>
    /// <param name="keyAlias">Alias tùy chọn cho tên Event (mặc định lấy tên Class). Dùng để map với string Type lưu trong DB Outbox.</param>
    public void Register<TEvent>(string topic, string? keyAlias = null)
    {
        var type = typeof(TEvent);
        var key = keyAlias ?? type.Name;
        _typeMappings[key] = (type, topic);
    }

    /// <summary>
    /// Thử lấy thông tin Event Type và Topic dựa trên tên Type (lấy từ DB Outbox).
    /// </summary>
    /// <param name="typeName">Tên loại sự kiện (thường là Class Name).</param>
    /// <param name="eventType">Output: Event Type thực tế.</param>
    /// <param name="topic">Output: Topic đích.</param>
    /// <returns>True nếu tìm thấy.</returns>
    public bool TryGet(string typeName, out Type eventType, out string topic)
    {
        if (_typeMappings.TryGetValue(typeName, out var mapping))
        {
            eventType = mapping.EventType;
            topic = mapping.Topic;
            return true;
        }

        eventType = null!;
        topic = null!;
        return false;
    }
}