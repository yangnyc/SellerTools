using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.AccountViewModels
{
    /// <summary>
    /// View model for external login provider authentication.
    /// Contains email address for associating external login with account.
    /// </summary>
    public class ExternalLoginViewModel
    {
        /// <summary>
        /// Gets or sets the email address to associate with external login.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
