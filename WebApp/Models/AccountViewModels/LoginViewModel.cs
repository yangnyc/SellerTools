using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.AccountViewModels
{
    /// <summary>
    /// View model for user login form.
    /// Contains user credentials and remember me option.
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// Gets or sets the username or email address for login.
        /// </summary>
        [Display(Name = "Email or Username")]
        public required string UserName { get; set; }

        /// <summary>
        /// Gets or sets the password for authentication.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to persist the login session.
        /// </summary>
        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }
}
