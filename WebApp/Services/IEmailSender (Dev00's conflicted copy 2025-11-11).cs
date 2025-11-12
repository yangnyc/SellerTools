using System.Threading.Tasks;

namespace WebApp.Services
{
    /// <summary>
    /// Interface for email sending functionality.
    /// Defines the contract for sending emails in the application.
    /// </summary>
    public interface IEmailSender
    {
        /// <summary>
        /// Sends an email asynchronously.
        /// </summary>
        /// <param name="email">Recipient email address.</param>
        /// <param name="subject">Email subject line.</param>
        /// <param name="message">Email message body.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendEmailAsync(string email, string subject, string message);
    }
}
