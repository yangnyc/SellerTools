using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace WebApp.Models.ManageViewModels
{
    /// <summary>
    /// View model for managing external login providers.
    /// Displays current linked accounts and available providers.
    /// </summary>
    public class ExternalLoginsViewModel
    {
        /// <summary>
        /// Gets or sets the list of currently linked external logins.
        /// </summary>
        public IList<UserLoginInfo>? CurrentLogins { get; set; }

        /// <summary>
        /// Gets or sets the list of available external login providers not yet linked.
        /// </summary>
        public IList<AuthenticationScheme>? OtherLogins { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the remove button should be displayed.
        /// </summary>
        public bool ShowRemoveButton { get; set; }

        /// <summary>
        /// Gets or sets the status message to display.
        /// </summary>
        public string? StatusMessage { get; set; }
    }
}
