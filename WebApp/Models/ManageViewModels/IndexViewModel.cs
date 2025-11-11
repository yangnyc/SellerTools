using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.ManageViewModels
{
    /// <summary>
    /// View model for the user account management index page.
    /// Contains user profile information and account status.
    /// </summary>
    public class IndexViewModel
    {
        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the email has been confirmed.
        /// </summary>
        public bool IsEmailConfirmed { get; set; }

        /// <summary>
        /// Gets or sets the user's email address.
        /// </summary>
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the user's phone number.
        /// </summary>
        [Phone]
        [Display(Name = "Phone number")]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the status message to display.
        /// </summary>
        public string? StatusMessage { get; set; }
    }
}
