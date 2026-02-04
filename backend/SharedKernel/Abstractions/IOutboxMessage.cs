using System;

namespace SharedKernel.Abstractions;

/// <summary>
/// Interface chung cho các Outbox Message của từng Service.
/// Giúp chuẩn hóa cấu trúc và hỗ trợ viết Extension Methods.
/// </summary>
public interface IOutboxMessage
{
    Guid Id { get; set; }
    string Type { get; set; }
    string Content { get; set; }
    DateTimeOffset OccurredOn { get; set; }
}
