using System;
using System.Collections.Generic;

namespace SharedKernel.Infrastructure.Kafka;

/// <summary>
/// Registry lưu trữ cấu hình mapping giữa Event Type, Topic và Handler xử lý.
/// Được sử dụng để tra cứu động (dynamic resolution) khi Consume tin nhắn.
/// </summary>
public class KafkaConsumerRegistry
{
    private readonly Dictionary<string, (Type EventType, Type HandlerType)> _topicMappings = new();

    /// <summary>
    /// Đăng ký một Topic với Event Type và Handler tương ứng.
    /// </summary>
    /// <typeparam name="TEvent">Loại Event (Class).</typeparam>
    /// <param name="topic">Tên Kafka Topic.</param>
    /// <param name="handlerType">Class Handler thực thi logic.</param>
    public void Register<TEvent>(string topic, Type handlerType)
    {
        _topicMappings[topic] = (typeof(TEvent), handlerType);
    }

    /// <summary>
    /// Thử lấy thông tin Event Type và Handler Type dựa trên Topic.
    /// </summary>
    /// <param name="topic">Tên Topic.</param>
    /// <param name="eventType">Output: Event Type.</param>
    /// <param name="handlerType">Output: Handler Type.</param>
    /// <returns>True nếu tìm thấy, False nếu không.</returns>
    public bool TryGet(string topic, out Type eventType, out Type handlerType)
    {
        if (_topicMappings.TryGetValue(topic, out var mapping))
        {
            eventType = mapping.EventType;
            handlerType = mapping.HandlerType;
            return true;
        }

        eventType = null!;
        handlerType = null!;
        return false;
    }

    /// <summary>
    /// Lấy danh sách tất cả các Topic đã đăng ký.
    /// </summary>
    public IEnumerable<string> GetAllTopics() => _topicMappings.Keys;
}
