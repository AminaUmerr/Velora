using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Velora.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody);
        Task SendOrderConfirmationAsync(string toEmail, string toName, string orderNumber, decimal grandTotal);
        Task SendShippingNotificationAsync(string toEmail, string toName, string orderNumber);
        Task SendWelcomeEmailAsync(string toEmail, string fullName);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(
                    _config["EmailSettings:SenderName"] ?? "Velora",
                    _config["EmailSettings:SenderEmail"] ?? "noreply@velora.com"));
                message.To.Add(new MailboxAddress(toName, toEmail));
                message.Subject = subject;
                message.Body = new TextPart("html") { Text = WrapInTemplate(htmlBody) };

                using var client = new SmtpClient();
                await client.ConnectAsync(
                    _config["EmailSettings:SmtpServer"] ?? "smtp.gmail.com",
                    int.Parse(_config["EmailSettings:Port"] ?? "587"),
                    SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(
                    _config["EmailSettings:SenderEmail"],
                    _config["EmailSettings:SenderPassword"]);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            }
        }

        public async Task SendOrderConfirmationAsync(string toEmail, string toName, string orderNumber, decimal grandTotal)
        {
            var body = $@"
                <h2 style='color:#C9A84C;'>Order Confirmed!</h2>
                <p>Hi <strong>{toName}</strong>,</p>
                <p>Thank you for your order. We're thrilled to have you shop with Velora.</p>
                <div style='background:#f8f8f8;padding:20px;border-radius:8px;margin:20px 0;'>
                    <p><strong>Order Number:</strong> #{orderNumber}</p>
                    <p><strong>Grand Total:</strong> PKR {grandTotal:N0}</p>
                    <p><strong>Status:</strong> Confirmed ✓</p>
                </div>
                <p>We'll send you another email once your order has been shipped.</p>
                <a href='#' style='background:#C9A84C;color:#fff;padding:12px 28px;text-decoration:none;border-radius:6px;display:inline-block;margin-top:10px;'>Track Your Order</a>";
            await SendEmailAsync(toEmail, toName, $"Order Confirmed – #{orderNumber} | Velora", body);
        }

        public async Task SendShippingNotificationAsync(string toEmail, string toName, string orderNumber)
        {
            var body = $@"
                <h2 style='color:#C9A84C;'>Your Order is on its Way!</h2>
                <p>Hi <strong>{toName}</strong>,</p>
                <p>Great news! Your Velora order <strong>#{orderNumber}</strong> has been shipped and is on its way to you.</p>
                <div style='background:#f8f8f8;padding:20px;border-radius:8px;margin:20px 0;'>
                    <p><strong>Order Number:</strong> #{orderNumber}</p>
                    <p><strong>Status:</strong> Shipped 🚚</p>
                    <p>Estimated delivery: 2-4 business days</p>
                </div>
                <a href='#' style='background:#C9A84C;color:#fff;padding:12px 28px;text-decoration:none;border-radius:6px;display:inline-block;margin-top:10px;'>Track Your Order</a>";
            await SendEmailAsync(toEmail, toName, $"Your Order #{orderNumber} Has Been Shipped | Velora", body);
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string fullName)
        {
            var body = $@"
                <h2 style='color:#C9A84C;'>Welcome to Velora!</h2>
                <p>Hi <strong>{fullName}</strong>,</p>
                <p>You've successfully joined the Velora family. We're excited to have you with us.</p>
                <p>Discover our latest collections of premium fashion, shoes, watches, bags and accessories.</p>
                <a href='/shop' style='background:#C9A84C;color:#fff;padding:12px 28px;text-decoration:none;border-radius:6px;display:inline-block;margin-top:10px;'>Start Shopping</a>";
            await SendEmailAsync(toEmail, fullName, "Welcome to Velora – Your Premium Fashion Destination", body);
        }

        private static string WrapInTemplate(string body) => $@"
<!DOCTYPE html>
<html>
<head><meta charset='utf-8'><meta name='viewport' content='width=device-width,initial-scale=1'></head>
<body style='margin:0;padding:0;background:#f4f4f4;font-family:Arial,sans-serif;'>
  <div style='max-width:600px;margin:40px auto;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 4px 20px rgba(0,0,0,0.08);'>
    <div style='background:#0D0D0D;padding:30px;text-align:center;'>
      <h1 style='color:#C9A84C;margin:0;font-size:28px;letter-spacing:4px;'>VELORA</h1>
      <p style='color:#999;margin:4px 0 0;font-size:12px;letter-spacing:2px;'>PREMIUM FASHION & LIFESTYLE</p>
    </div>
    <div style='padding:40px 32px;color:#333;line-height:1.7;font-size:15px;'>{body}</div>
    <div style='background:#f8f8f8;padding:20px;text-align:center;border-top:1px solid #eee;'>
      <p style='color:#999;font-size:12px;margin:0;'>© {DateTime.Now.Year} Velora. All rights reserved.</p>
      <p style='color:#999;font-size:12px;margin:4px 0 0;'>Premium Fashion & Lifestyle Store</p>
    </div>
  </div>
</body>
</html>";
    }
}
