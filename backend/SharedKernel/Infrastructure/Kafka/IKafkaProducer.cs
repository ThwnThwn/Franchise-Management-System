using System.Threading.Tasks;

namespace SharedKernel.Infrastructure.Kafka;

/// <summary>
/// Interface định nghĩa contract cho Kafka Producer.
/// Sử dụng để publish (gửi) messages/events lên Kafka topics.
/// </summary>
public interface IKafkaProducer
{
    /// <summary>
    /// Publish một message lên Kafka topic chỉ định.
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu của message (phải là class)</typeparam>
    /// <param name="topic">Tên topic Kafka đích</param>
    /// <param name="message">Nội dung message cần gửi</param>
    /// <returns>Task hoàn thành khi message đã được gửi</returns>
    Task PublishAsync<T>(string topic, T message) where T : class;
}
