using System.Net;
using System.Net.Mail;

namespace Real_Estate_App.Services
{
    public interface IEmailService
    {
        Task SendPurchaseConfirmationAsync(string toEmail, string buyerName, string propertyName, string propertyAddress, decimal price, DateTime purchaseDate);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendPurchaseConfirmationAsync(string toEmail, string buyerName, string propertyName, string propertyAddress, decimal price, DateTime purchaseDate)
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings");
            var host = smtpSettings["Host"] ?? "smtp.gmail.com";
            var port = int.Parse(smtpSettings["Port"] ?? "587");
            var senderEmail = smtpSettings["SenderEmail"] ?? "";
            var senderPassword = smtpSettings["SenderPassword"] ?? "";
            var senderName = smtpSettings["SenderName"] ?? "Real Estate App";

            var subject = $"Purchase Confirmation - {propertyName}";
            var body = $@"
<html>
<body style='font-family: Arial, sans-serif; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <h1 style='color: #2c3e50; border-bottom: 2px solid #3498db; padding-bottom: 10px;'>Purchase Confirmation</h1>

        <p>Dear <strong>{buyerName}</strong>,</p>

        <p>Thank you for your purchase! Your transaction has been successfully processed.</p>

        <div style='background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0;'>
            <h2 style='color: #2c3e50; margin-top: 0;'>Property Details</h2>
            <table style='width: 100%; border-collapse: collapse;'>
                <tr>
                    <td style='padding: 8px 0; font-weight: bold; width: 140px;'>Property:</td>
                    <td style='padding: 8px 0;'>{propertyName}</td>
                </tr>
                <tr>
                    <td style='padding: 8px 0; font-weight: bold;'>Address:</td>
                    <td style='padding: 8px 0;'>{propertyAddress}</td>
                </tr>
                <tr>
                    <td style='padding: 8px 0; font-weight: bold;'>Price:</td>
                    <td style='padding: 8px 0;'>{price:C}</td>
                </tr>
                <tr>
                    <td style='padding: 8px 0; font-weight: bold;'>Date:</td>
                    <td style='padding: 8px 0;'>{purchaseDate:dddd, dd MMMM yyyy}</td>
                </tr>
            </table>
        </div>

        <p>Our team will be in touch within 2-3 business days to arrange the next steps for your property settlement.</p>

        <p>If you have any questions, please don't hesitate to contact us.</p>

        <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;' />
        <p style='color: #888; font-size: 12px;'>This is an automated message from Real Estate App. Please do not reply directly to this email.</p>
    </div>
</body>
</html>";

            try
            {
                using var client = new SmtpClient(host, port);
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(senderEmail, senderPassword);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Purchase confirmation email sent to {Email} for property {Property}", toEmail, propertyName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send purchase confirmation email to {Email}", toEmail);
                throw;
            }
        }
    }
}
