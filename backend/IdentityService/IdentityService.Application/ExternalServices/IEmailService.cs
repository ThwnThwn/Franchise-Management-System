namespace IdentityService.Application.ExternalServices;

public interface IEmailService
{
    Task SendOtpEmailAsync(string toEmail, string otp);
}
