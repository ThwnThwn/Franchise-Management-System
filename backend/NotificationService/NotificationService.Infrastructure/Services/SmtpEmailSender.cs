using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using MimeKit;
using NotificationService.Application.Abstractions;
using NotificationService.Application.Model;
using NotificationService.Infrastructure.Configurations;
using SharedKernel.Events;
using System.Globalization;
using System.Text.RegularExpressions;
using static SharedKernel.Events.OrderStatusEvent;
using static System.Net.Mime.MediaTypeNames;
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
                    Cà Hong Bạn — brewed with care 
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
    public async Task SendOrderStatusEmailAsync(string toEmail, OrderStatusEvent orderEvent)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        message.To.Add(new MailboxAddress("", toEmail));

        // Subject depends on event
        message.Subject = "Order status updated";

        var mainMessage =  $"Order <b>#{orderEvent.OrderCode}</b> is now <b>{orderEvent.NewOrderStatus}</b>.";
        var itemsHtml = string.Join("", orderEvent.Items.Select(i => $@"
        <tr>
            <td style='padding: 8px 0;'>{i.Sku}</td>
            <td style='padding: 8px 0; text-align: center;'>{i.Quantity}</td>
            <td style='text-align: right;'>
                    {(i.Quantity * i.UnitPrice).ToString("N0")} VND
                </td>   
        </tr>
    "));
        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = $@"
<div style='font-family: Verdana, sans-serif; background-color: #f8f1e7; padding: 40px;'>

    <div style='max-width: 520px; margin: auto; background: #fff;
                border-radius: 16px; padding: 30px;
                box-shadow: 0 6px 18px rgba(0,0,0,0.12);'>

        <div style='text-align: center; margin-bottom: 25px;'>
            <h1 style='margin: 0; font-size: 28px; color: #4b2e2a;'>
                Cà Hong Bạn
            </h1>
            <p style='margin: 6px 0 0; font-size: 13px; color: #a67c52;'>
                Fresh drinks. Order status updated.
            </p>
        </div>

        <p style='font-size: 15px; color: #333; line-height: 1.6;'>
            Xin chào bạn,<br/>
            <b>Your order #{orderEvent.OrderCode}</b> is now {orderEvent.NewOrderStatus}.
        </p>

        <div style='margin: 25px 0; padding: 18px;
                    background: #faf7f3; border-radius: 12px;'>

            <p style='margin: 6px 0; font-size: 14px;'>
                <b>Order ID:</b> {orderEvent.OrderCode}
            </p>
            <table style='width: 100%; font-size: 14px; border-collapse: collapse;'>
                <thead>
                    <tr style='border-bottom: 1px solid #ddd;'>
                        <th style='text-align: left; padding: 8px 0;'>Name</th>
                        <th style='text-align: center; padding: 8px 0;'>Qty</th>
                        <th style='text-align: right; padding: 8px 0;'>Total</th>
                    </tr>
                </thead>

                <tbody>
                    {itemsHtml}
                </tbody>

                <tfoot>
                    <!-- Tax -->
                    <tr>
                        <td colspan=""2"" style=""padding-top: 10px; text-align: right;"">
                            Tax (10%):
                        </td>
                        <td style=""padding-top: 10px; text-align: right;"">
                            {orderEvent.Tax.ToString("N0")} VND
                        </td>
                    </tr>
                    <tr>
                        <td colspan='2' style='padding-top: 12px; font-weight: bold; text-align: right;'>
                            Total Amount:
                        </td>
                        <td style='padding-top: 12px; font-weight: bold; text-align: right; color: #4b2e2a; font-size: 15px;'>
                            {orderEvent.TotalAmount.ToString("N0")} VND
                        </td>
                    </tr>
                </tfoot>
            </table>

        </div>

        <p style='margin-top: 20px; font-size: 14px; color: #555;'>
            Bạn sẽ nhận được thông báo tiếp theo khi trạng thái đơn hàng thay đổi.
        </p>

        <div style='margin-top: 35px; text-align: center; font-size: 12px; color: #aaa;'>
            <p style='margin: 0;'>
                Cà Hong Bạn — brewed with care
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
    public async Task SendOrderPlacedEmailAsync(string toEmail,OrderPlacedEmailModel order)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = "Your order has been placed successfully";

        var itemsHtml = string.Join("", order.Items.Select(i => $@"
        <tr>
            <td style='padding: 8px 0;'>{i.Sku}</td>
            <td style='padding: 8px 0; text-align: center;'>{i.Quantity}</td>
            <td style='text-align: right;'>
                    {(i.Quantity * i.UnitPrice).ToString("N0")} VND
                </td>   
        </tr>
    "));

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = $@"
<div style='font-family: Verdana, sans-serif; background-color: #f8f1e7; padding: 40px;'>

    <div style='max-width: 520px; margin: auto; background: #fff;
                border-radius: 16px; padding: 30px;
                box-shadow: 0 6px 18px rgba(0,0,0,0.12);'>

        <div style='text-align: center; margin-bottom: 25px;'>
            <h1 style='margin: 0; font-size: 28px; color: #4b2e2a;'>
                Cà Hông Bạn
            </h1>
            <p style='margin: 6px 0 0; font-size: 13px; color: #a67c52;'>
                Fresh drinks. Order status updated.
            </p>
        </div>

        <p style='font-size: 15px; color: #333; line-height: 1.6;'>
            Xin chào bạn,<br/>
            <b>Your order #{order.OrderCode}</b> has been placed successfully.
        </p>

        <div style='margin: 25px 0; padding: 18px;
                    background: #faf7f3; border-radius: 12px;'>

            <p style='margin: 6px 0; font-size: 14px;'>
                <b>Order ID:</b> {order.OrderCode}
            </p>
            <table style='width: 100%; font-size: 14px; border-collapse: collapse;'>
                <thead>
                    <tr style='border-bottom: 1px solid #ddd;'>
                        <th style='text-align: left; padding: 8px 0;'>Name</th>
                        <th style='text-align: center; padding: 8px 0;'>Qty</th>
                        <th style='text-align: right; padding: 8px 0;'>Total</th>
                    </tr>
                </thead>

                <tbody>
                    {itemsHtml}
                </tbody>
                <tfoot>
                    <!-- Tax -->
                    <tr>
                        <td colspan=""2"" style=""padding-top: 10px; text-align: right;"">
                            Tax (10%):
                        </td>
                        <td style=""padding-top: 10px; text-align: right;"">
                            {order.Tax.ToString("N0")} VND
                        </td>
                    </tr>
                    <tr>
                        <td colspan='2' style='padding-top: 12px; font-weight: bold; text-align: right;'>
                            Total Amount:
                        </td>
                        <td style='padding-top: 12px; font-weight: bold; text-align: right; color: #4b2e2a; font-size: 15px;'>
                            {order.TotalAmount.ToString("N0")} VND
                        </td>
                    </tr>
                </tfoot>
            </table>

        </div>

        <p style='margin-top: 20px; font-size: 14px; color: #555;'>
            Bạn sẽ nhận được thông báo tiếp theo khi trạng thái đơn hàng thay đổi.
        </p>

        <div style='margin-top: 35px; text-align: center; font-size: 12px; color: #aaa;'>
            <p style='margin: 0;'>
                Cà Hong Bạn — brewed with care
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