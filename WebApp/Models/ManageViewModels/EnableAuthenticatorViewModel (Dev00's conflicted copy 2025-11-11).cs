using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.ManageViewModels
{
    /// <summary>
    /// View model for enabling two-factor authentication.
    /// Contains verification code and authenticator setup information.
    /// </summary>
    public class EnableAuthenticatorViewModel
    {
        /// <summary>
        /// Gets or sets the verification code from authenticator app.
        /// Must be 6-7 characters.
        /// </summary>
        [Required]
        [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "Verification Code")]
        public string? Code { get; set; }

        /// <summary>
        /// Gets or sets the shared secret key for authenticator app setup.
        /// Not bound to model during request.
        /// </summary>
        [BindNever]
        public string? SharedKey { get; set; }

        /// <summary>
        /// Gets or sets the authenticator URI for QR code generation.
        /// Not bound to model during request.
        /// </summary>
        [BindNever]
        public string? AuthenticatorUri { get; set; }
    }
}
