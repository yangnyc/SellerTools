using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.ManageViewModels
{
    /// <summary>
    /// View model for changing user password.
    /// Requires current password and new password with confirmation.
    /// </summary>
    public class ChangePasswordViewModel
    {
        /// <summary>
        /// Gets or sets the current password for verification.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string? OldPassword { get; set; }

        /// <summary>
        /// Gets or sets the new password.
        /// Must be between 6 and 100 characters.
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string? NewPassword { get; set; }

        /// <summary>
        /// Gets or sets the new password confirmation.
        /// Must match the NewPassword field.
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }

        /// <summary>
        /// Gets or sets the status message to display.
        /// </summary>
        public string? StatusMessage { get; set; }
    }
}
