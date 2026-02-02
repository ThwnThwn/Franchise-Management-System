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
    <div style='font-family: Verdana, sans-serif; background-color: #f8f1e7; padding: 40px;'>

        <div style='max-width: 520px; margin: auto; background: #fff;
                    border-radius: 16px; padding: 30px;
                    box-shadow: 0 6px 18px rgba(0,0,0,0.12);'>

            <!-- Logo / Title -->
            <div style='text-align: center; margin-bottom: 25px;'>
                <h1 style='margin: 0; font-size: 28px; color: #4b2e2a;'>
                     Cà Hong bạn
                </h1>
                <p style='margin: 6px 0 0; font-size: 13px; color: #a67c52;'>
                    A warm cup. A safe login.
                </p>
            </div>

            <!-- Message -->
            <p style='font-size: 15px; color: #333; line-height: 1.6;'>
                Xin chào bạn,<br/>
                Chúng mình vừa nhận được yêu cầu xác thực tài khoản.
            </p>

            <p style='font-size: 15px; color: #333;'>
                Đây là mã OTP của bạn:
            </p>

            <!-- OTP Code -->
            <div style='text-align: center; margin: 30px 0;'>
                <div style='display: inline-block;
                            background: #4b2e2a;
                            padding: 18px 35px;
                            border-radius: 14px;'>
                    
                    <span style='font-size: 34px;
                                 color: #fff;
                                 font-weight: bold;
                                 letter-spacing: 8px;'>
                        {otp}
                    </span>
                </div>
            </div>

            <!-- Expire Notice -->
            <p style='font-size: 14px; color: #555;'>
                ⏳ Mã này chỉ có hiệu lực trong <b>10 phút</b>.
            </p>

            <p style='font-size: 13px; color: #777;'>
                Nếu bạn không yêu cầu OTP, vui lòng bỏ qua email này.
            </p>

            <!-- Footer -->
            <div style='margin-top: 35px; text-align: center; font-size: 12px; color: #aaa;'>
                <p style='margin: 0;'>
                    Cà Hông Bạn — brewed with care 
                </p>
                <p style='margin: 5px 0 0;'>
                    © {DateTime.Now.Year}
                </p>
            </div>

        </div>
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
    public async Task SendLowStockEmailAsync(string toEmail, LowStockEvent lowStockEvent)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = "Low Stock Alert";
        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = $@"
            <div style='font-family: Arial, sans-serif; padding: 20px;'>
                <h2 style='color: #E53935;'>Low Stock Alert</h2>
                <p>The stock for the following ingredient is low:</p>
                <p><strong>Ingredient:</strong> {lowStockEvent.IngredientName}</p>
                <p><strong>Current Stock:</strong> {lowStockEvent.CurrentStock}</p>
                <p><strong>Minimum Threshold:</strong> {lowStockEvent.MinThreshold}</p>
                <p>Please restock as soon as possible.</p>
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