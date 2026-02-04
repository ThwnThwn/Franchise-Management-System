using Microsoft.Extensions.Logging;
using SharedKernel.Events;
using SharedKernel.Infrastructure.Kafka;
using System.Threading.Tasks;

namespace NotificationService.Application.EventHandlers;

/// <summary>
/// Xử lý sự kiện yêu cầu Reset mật khẩu.
/// Nhiệm vụ: Gửi email link reset password.
/// </summary>
public class PasswordResetHandler : IIntegrationEventHandler<PasswordResetRequestedEvent>
{
    private readonly ILogger<PasswordResetHandler> _logger;

    public PasswordResetHandler(ILogger<PasswordResetHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(PasswordResetRequestedEvent @event)
    {
        _logger.LogInformation("Password reset event received for {Email}", @event.Email);
        return Task.CompletedTask;
    }
}
