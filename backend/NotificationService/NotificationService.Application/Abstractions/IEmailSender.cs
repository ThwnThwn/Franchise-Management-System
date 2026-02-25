using NotificationService.Application.Model;
using SharedKernel.Events;
namespace NotificationService.Application.Abstractions;
public interface IEmailSender
{
    Task SendOtpEmailAsync(string toEmail, string otp);
    Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
    Task SendOrderStatusEmailAsync(string toEmail, OrderStatusEvent orderEvent);
    Task SendOrderPlacedEmailAsync(string toEmail, OrderPlacedEmailModel order);
    Task SendLowStockEmailAsync(string toEmail, LowStockEvent lowStockEvent);
}