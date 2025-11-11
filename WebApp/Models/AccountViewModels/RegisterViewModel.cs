using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.AccountViewModels
{
    /// <summary>
    /// View model for user registration form.
    /// Contains email, password, and password confirmation fields.
    /// </summary>
    public class RegisterViewModel
    {
        /// <summary>
        /// Gets or sets the email address for the new account.
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the password for the new account.
        /// Must be at least 6 characters long.
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        /// <summary>
        /// Gets or sets the password confirmation.
        /// Must match the Password field.
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }
    }
}
