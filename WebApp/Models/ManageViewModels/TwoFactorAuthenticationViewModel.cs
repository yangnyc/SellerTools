namespace WebApp.Models.ManageViewModels
{
    /// <summary>
    /// View model for two-factor authentication status page.
    /// Displays current 2FA configuration and recovery code status.
    /// </summary>
    public class TwoFactorAuthenticationViewModel
    {
        /// <summary>
        /// Gets or sets a value indicating whether an authenticator is configured.
        /// </summary>
        public bool HasAuthenticator { get; set; }

        /// <summary>
        /// Gets or sets the number of unused recovery codes remaining.
        /// </summary>
        public int RecoveryCodesLeft { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether two-factor authentication is enabled.
        /// </summary>
        public bool Is2faEnabled { get; set; }
    }
}
