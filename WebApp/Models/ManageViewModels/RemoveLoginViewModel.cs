namespace WebApp.Models.ManageViewModels
{
    /// <summary>
    /// View model for removing an external login provider link.
    /// Contains provider identification information.
    /// </summary>
    public class RemoveLoginViewModel
    {
        /// <summary>
        /// Gets or sets the name of the login provider (e.g., "Google", "Facebook").
        /// </summary>
        public string? LoginProvider { get; set; }

        /// <summary>
        /// Gets or sets the provider-specific user key.
        /// </summary>
        public string? ProviderKey { get; set; }
    }
}
