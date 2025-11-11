using System.Threading.Tasks;

namespace WebApp.Services
{
    /// <summary>
    /// Email sender service implementation.
    /// Currently returns a completed task without sending actual emails (stub implementation).
    /// </summary>
    public class EmailSender : IEmailSender
    {
        /// <summary>
        /// Sends an email asynchronously.
        /// </summary>
        /// <param name="email">Recipient email address.</param>
        /// <param name="subject">Email subject line.</param>
        /// <param name="message">Email message body.</param>
        /// <returns>A completed task.</returns>
        public Task SendEmailAsync(string email, string subject, string message)
        {
            return Task.CompletedTask;
        }
    }
}
