using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using NotificationService.Application.Abstractions;
using NotificationService.Infrastructure.Configurations;
using SharedKernel.Events;
namespace NotificationService.Infrastructure.Services;
public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpSettings _settings;
    public SmtpEmailSender(IOptions<SmtpSettings> settings)
    {
        _settings = settings.Value;
    }
    public async Task SendOtpEmailAsync(string toEmail, string otp)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = "OTP Verification Code";
        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = $@"
            <div style='font-family: Arial, sans-serif; padding: 20px;'>
                <h2>OTP Verification</h2>
                <p>Your OTP code is:</p>
                <h1 style='color: #4CAF50; letter-spacing: 5px;'>{otp}</h1>
                <p>This code will expire in 10 minutes.</p>
            </div>"
        };
        message.Body = bodyBuilder.ToMessageBody();
        await SendEmailInternalAsync(message);
    }
    public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = "Password Reset Request";
        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = $@"
            <div style='font-family: Arial, sans-serif; padding: 20px;'>
                <h2>Password Reset</h2>
                <p>Click the link below to reset your password:</p>
                <a href='{resetLink}' style='background: #4CAF50; color: white; padding: 10px 20px; text-decoration: none;'>
                    Reset Password
                </a>
                <p>This link will expire in 1 hour.</p>
            </div>"
        };
        message.Body = bodyBuilder.ToMessageBody();
        await SendEmailInternalAsync(message);
    }
    public async Task SendOrderConfirmationEmailAsync(string toEmail, OrderCreatedEvent orderEvent)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = "Order Confirmation";
        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = $@"
            <div style='font-family: Arial, sans-serif; padding: 20px;'>
                <h2>Order Confirmed!</h2>
                <p>Thank you for your order.</p>
                <p><strong>Order ID:</strong> {orderEvent.OrderId}</p>
                <p><strong>Summary:</strong> {orderEvent.OrderSummary}</p>
                <p><strong>Total:</strong> ${orderEvent.TotalAmount:F2}</p>
                <p>We'll process your order shortly.</p>
            </div>"
        };
        message.Body = bodyBuilder.ToMessageBody();
        await SendEmailInternalAsync(message);
    }
    private async Task SendEmailInternalAsync(MimeMessage message)
    {
        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_settings.Username, _settings.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}