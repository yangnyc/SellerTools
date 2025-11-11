namespace WebApp.Models.ManageViewModels
{
    /// <summary>
    /// View model for displaying recovery codes.
    /// Shows backup codes for two-factor authentication when user loses access to authenticator.
    /// </summary>
    public class ShowRecoveryCodesViewModel
    {
        /// <summary>
        /// Gets or sets the array of recovery codes to display.
        /// </summary>
        public string[]? RecoveryCodes { get; set; }
    }
}
