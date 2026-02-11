using FranchiseService.Domain.Commons;

namespace FranchiseService.Domain.Entities
{
    /// <summary>
    /// Lưu trữ các event cần publish ra Kafka để đảm bảo tính toàn vẹn (Outbox Pattern).
    /// Bảng: outbox_messages
    /// </summary>
    public class OutboxMessage : IEntity
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Loại Event (VD: ShiftCreated)
        /// </summary>
        public string Type { get; set; } = null!;

        /// <summary>
        /// Nội dung JSON
        /// </summary>
        public string Content { get; set; } = null!;

        /// <summary>
        /// Thời điểm sự kiện xảy ra
        /// </summary>
        public DateTimeOffset OccurredOn { get; set; }

        /// <summary>
        /// Thời điểm đã xử lý (Publish thành công)
        /// </summary>
        public DateTimeOffset? ProcessedOn { get; set; }

        public string? Error { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
