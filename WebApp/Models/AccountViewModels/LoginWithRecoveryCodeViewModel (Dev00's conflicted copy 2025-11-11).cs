using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.AccountViewModels
{
    /// <summary>
    /// View model for recovery code login.
    /// Used when user doesn't have access to their authenticator app.
    /// </summary>
    public class LoginWithRecoveryCodeViewModel
    {
        /// <summary>
        /// Gets or sets the backup recovery code for authentication.
        /// </summary>
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Recovery Code")]
        public string? RecoveryCode { get; set; }
    }
}