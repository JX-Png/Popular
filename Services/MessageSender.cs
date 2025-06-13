using System.Diagnostics; // For Debug.WriteLine

namespace PopularBookstore.Services
{
    public class MessageSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            Debug.WriteLine($"Email to: {email}");
            Debug.WriteLine($"Subject: {subject}");
            Debug.WriteLine($"Message: {htmlMessage}");
            return Task.CompletedTask;
        }
    }
}