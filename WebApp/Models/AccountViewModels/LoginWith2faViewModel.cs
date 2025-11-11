using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.AccountViewModels
{
    /// <summary>
    /// View model for two-factor authentication login.
    /// Contains authenticator code and remember options.
    /// </summary>
    public class LoginWith2faViewModel
    {
        /// <summary>
        /// Gets or sets the two-factor authentication code from authenticator app.
        /// Must be 6-7 characters.
        /// </summary>
        [Required]
        [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "Authenticator code")]
        public string? TwoFactorCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to remember this machine for future logins.
        /// </summary>
        [Display(Name = "Remember this machine")]
        public bool RememberMachine { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to persist the login session.
        /// </summary>
        public bool RememberMe { get; set; }
    }
}