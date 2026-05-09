using System.Net;
using System.Net.Mail;

namespace Real_Estate_App.Services
{
    public interface IEmailService
    {
        Task<bool> SendPurchaseRequestReceivedAsync(string toEmail, string buyerName, string propertyName, string propertyAddress, decimal price, DateTime requestDate);
        Task<bool> SendPurchaseApprovedAsync(string toEmail, string buyerName, string propertyName, string propertyAddress, decimal price, DateTime approvedDate);
        Task<bool> SendPurchaseRejectedAsync(string toEmail, string buyerName, string propertyName, string propertyAddress, decimal price, DateTime rejectedDate, string? reason);
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

        public Task<bool> SendPurchaseRequestReceivedAsync(string toEmail, string buyerName, string propertyName, string propertyAddress, decimal price, DateTime requestDate)
        {
            var subject = $"Purchase Request Received - {propertyName}";
            var body = BuildShell(
                heading: "Purchase Request Received",
                headingColor: "#2c3e50",
                buyerName: buyerName,
                introHtml: "<p>Thank you for your purchase request! Your request is now <strong>awaiting review</strong> by one of our Transaction Admins.</p><p>You will receive a follow-up email once a decision has been made.</p>",
                statusLabel: "Pending Approval",
                statusColor: "#f39c12",
                propertyName: propertyName,
                propertyAddress: propertyAddress,
                price: price,
                date: requestDate,
                dateLabel: "Request Date",
                extraHtml: null);
            return SendAsync(toEmail, subject, body);
        }

        public Task<bool> SendPurchaseApprovedAsync(string toEmail, string buyerName, string propertyName, string propertyAddress, decimal price, DateTime approvedDate)
        {
            var subject = $"Purchase Approved - {propertyName}";
            var body = BuildShell(
                heading: "Purchase Approved",
                headingColor: "#28a745",
                buyerName: buyerName,
                introHtml: "<p>Great news! Your purchase request has been <strong>approved</strong>.</p><p>Our team will be in touch within 2-3 business days to arrange the next steps for your property settlement.</p>",
                statusLabel: "Approved",
                statusColor: "#28a745",
                propertyName: propertyName,
                propertyAddress: propertyAddress,
                price: price,
                date: approvedDate,
                dateLabel: "Approved Date",
                extraHtml: null);
            return SendAsync(toEmail, subject, body);
        }

        public Task<bool> SendPurchaseRejectedAsync(string toEmail, string buyerName, string propertyName, string propertyAddress, decimal price, DateTime rejectedDate, string? reason)
        {
            var subject = $"Purchase Request Declined - {propertyName}";
            var reasonBlock = string.IsNullOrWhiteSpace(reason)
                ? string.Empty
                : $"<div style='background-color:#fdecea;border-left:4px solid #c0392b;padding:12px 16px;margin:20px 0;'><strong>Reason provided:</strong><br/>{System.Net.WebUtility.HtmlEncode(reason)}</div>";
            var body = BuildShell(
                heading: "Purchase Request Declined",
                headingColor: "#c0392b",
                buyerName: buyerName,
                introHtml: "<p>We're sorry to let you know that your purchase request has been <strong>declined</strong> by our Transaction Admin team.</p>",
                statusLabel: "Rejected",
                statusColor: "#c0392b",
                propertyName: propertyName,
                propertyAddress: propertyAddress,
                price: price,
                date: rejectedDate,
                dateLabel: "Decision Date",
                extraHtml: reasonBlock);
            return SendAsync(toEmail, subject, body);
        }

        private string BuildShell(string heading, string headingColor, string buyerName, string introHtml, string statusLabel, string statusColor, string propertyName, string propertyAddress, decimal price, DateTime date, string dateLabel, string? extraHtml)
        {
            return $@"
<html>
<body style='font-family: Arial, sans-serif; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <h1 style='color: {headingColor}; border-bottom: 2px solid {headingColor}; padding-bottom: 10px;'>{heading}</h1>

        <p>Dear <strong>{System.Net.WebUtility.HtmlEncode(buyerName)}</strong>,</p>

        {introHtml}

        <div style='background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0;'>
            <h2 style='color: #2c3e50; margin-top: 0;'>Property Details</h2>
            <table style='width: 100%; border-collapse: collapse;'>
                <tr>
                    <td style='padding: 8px 0; font-weight: bold; width: 140px;'>Property:</td>
                    <td style='padding: 8px 0;'>{System.Net.WebUtility.HtmlEncode(propertyName)}</td>
                </tr>
                <tr>
                    <td style='padding: 8px 0; font-weight: bold;'>Address:</td>
                    <td style='padding: 8px 0;'>{System.Net.WebUtility.HtmlEncode(propertyAddress)}</td>
                </tr>
                <tr>
                    <td style='padding: 8px 0; font-weight: bold;'>Price:</td>
                    <td style='padding: 8px 0;'>{price:C}</td>
                </tr>
                <tr>
                    <td style='padding: 8px 0; font-weight: bold;'>{dateLabel}:</td>
                    <td style='padding: 8px 0;'>{date:dddd, dd MMMM yyyy}</td>
                </tr>
                <tr>
                    <td style='padding: 8px 0; font-weight: bold;'>Status:</td>
                    <td style='padding: 8px 0; color: {statusColor}; font-weight: bold;'>{statusLabel}</td>
                </tr>
            </table>
        </div>

        {extraHtml}

        <p>If you have any questions, please don't hesitate to contact us.</p>

        <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;' />
        <p style='color: #888; font-size: 12px;'>This is an automated message from Real Estate App. Please do not reply directly to this email.</p>
    </div>
</body>
</html>";
        }

        private async Task<bool> SendAsync(string toEmail, string subject, string htmlBody)
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings");
            var host = smtpSettings["Host"] ?? "smtp.gmail.com";
            var port = int.Parse(smtpSettings["Port"] ?? "587");
            var senderEmail = smtpSettings["SenderEmail"] ?? "";
            var senderPassword = smtpSettings["SenderPassword"] ?? "";
            var senderName = smtpSettings["SenderName"] ?? "Real Estate App";

            if (string.IsNullOrWhiteSpace(senderEmail) || string.IsNullOrWhiteSpace(senderPassword))
            {
                _logger.LogWarning("SMTP sender credentials are not configured. Skipping email to {Email}. Set SmtpSettings:SenderEmail and SmtpSettings:SenderPassword in appsettings.Development.json to enable email.", toEmail);
                return false;
            }

            try
            {
                using var client = new SmtpClient(host, port);
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(senderEmail, senderPassword);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent to {Email}: {Subject}", toEmail, subject);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}: {Subject}", toEmail, subject);
                return false;
            }
        }
    }
}
