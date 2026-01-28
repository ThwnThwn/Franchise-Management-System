using IdentityService.Application.ExternalServices;
using IdentityService.Application.AppConfigurations;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace IdentityService.Infrastructure.ExternalServices;

public class EmailService : IEmailService
{
    private readonly SmtpSettings _smtpSettings;

    public EmailService(IOptions<SmtpSettings> smtpSettings)
    {
        _smtpSettings = smtpSettings.Value;
    }

    public async Task SendOtpEmailAsync(string toEmail, string otp)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_smtpSettings.FromName, _smtpSettings.FromEmail));
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
                    ☕ Cà Hong bạn
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
                    Cà Hông Bạn — brewed with care ☕
                </p>
                <p style='margin: 5px 0 0;'>
                    © {DateTime.Now.Year}
                </p>
            </div>

        </div>
    </div>"
};


        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
