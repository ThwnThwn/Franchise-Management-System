using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Abstractions;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Infrastructure.Data;
using static SharedKernel.Events.OrderStatusEvent;
namespace NotificationService.Infrastructure.Repositories;
public class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _context;
    public NotificationRepository(NotificationDbContext context)
    {
        _context = context;
    }
    public async Task<NotificationLog> CreateAsync(NotificationLog log)
    {
        _context.NotificationLogs.Add(log);
        await _context.SaveChangesAsync();
        return log;
    }
    public async Task UpdateAsync(NotificationLog log)
    {
        _context.NotificationLogs.Update(log);
        await _context.SaveChangesAsync();
    }
    public async Task<NotificationLog?> GetByIdAsync(Guid id)
    {
        return await _context.NotificationLogs.FindAsync(id);
    }
    public async Task<List<NotificationLog>> GetByRecipientAsync(string recipient)
    {
        return await _context.NotificationLogs
            .Where(n => n.Recipient == recipient)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> HasProcessedAsync(string referenceId, NotificationType type)
    {
        return await _context.NotificationLogs.AnyAsync(x =>
            x.ReferenceId == referenceId &&
            x.Type == type &&
            x.Status == NotificationStatus.Sent
        );
    }
}