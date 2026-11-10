using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using static SharedKernel.Events.OrderStatusEvent;
namespace NotificationService.Application.Abstractions;
public interface INotificationRepository
{
    Task<NotificationLog> CreateAsync(NotificationLog log);
    Task UpdateAsync(NotificationLog log);
    Task<NotificationLog?> GetByIdAsync(Guid id);
    Task<List<NotificationLog>> GetByRecipientAsync(string recipient);
    Task<bool> HasProcessedAsync(string referenceId, NotificationType type); // For Idempotency

}