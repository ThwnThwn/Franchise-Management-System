using NotificationService.Domain.Entities;
namespace NotificationService.Application.Abstractions;
public interface INotificationRepository
{
    Task<NotificationLog> CreateAsync(NotificationLog log);
    Task UpdateAsync(NotificationLog log);
    Task<NotificationLog?> GetByIdAsync(Guid id);
    Task<List<NotificationLog>> GetByRecipientAsync(string recipient);
    Task<bool> HasProcessedAsync(string referenceId); // For Idempotency
}