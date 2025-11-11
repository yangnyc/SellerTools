using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.AccountViewModels
{
    /// <summary>
    /// View model for the forgot password page.
    /// Collects user email to send password reset link.
    /// </summary>
    public class ForgotPasswordViewModel
    {
        /// <summary>
        /// Gets or sets the email address for password reset.
        /// </summary>
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
    }
}
