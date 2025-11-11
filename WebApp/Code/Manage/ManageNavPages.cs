using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;

namespace WebApp.Code.Manage
{
    /// <summary>
    /// Helper class for managing navigation state in user account management pages.
    /// Provides methods to determine active navigation links and page states.
    /// </summary>
    public static class ManageNavPages
    {
        /// <summary>
        /// Gets the ViewData key for storing the active page identifier.
        /// </summary>
        public static string ActivePageKey => "ActivePage";

        /// <summary>
        /// Gets the page identifier for the Index page.
        /// </summary>
        public static string Index => "Index";

        /// <summary>
        /// Gets the page identifier for the Change Password page.
        /// </summary>
        public static string ChangePassword => "ChangePassword";

        /// <summary>
        /// Gets the page identifier for the External Logins page.
        /// </summary>
        public static string ExternalLogins => "ExternalLogins";

        /// <summary>
        /// Gets the page identifier for the Two-Factor Authentication page.
        /// </summary>
        public static string TwoFactorAuthentication => "TwoFactorAuthentication";

        /// <summary>
        /// Gets the navigation CSS class for the Index page.
        /// </summary>
        /// <param name="viewContext">The view context.</param>
        /// <returns>"active" if current page is Index, otherwise null.</returns>
        public static string? IndexNavClass(ViewContext viewContext)
        {
            return PageNavClass(viewContext, Index);
        }

        /// <summary>
        /// Gets the navigation CSS class for the Change Password page.
        /// </summary>
        /// <param name="viewContext">The view context.</param>
        /// <returns>"active" if current page is ChangePassword, otherwise null.</returns>
        public static string? ChangePasswordNavClass(ViewContext viewContext)
        {
            return PageNavClass(viewContext, ChangePassword);
        }

        /// <summary>
        /// Gets the navigation CSS class for the External Logins page.
        /// </summary>
        /// <param name="viewContext">The view context.</param>
        /// <returns>"active" if current page is ExternalLogins, otherwise null.</returns>
        public static string? ExternalLoginsNavClass(ViewContext viewContext)
        {
            return PageNavClass(viewContext, ExternalLogins);
        }

        /// <summary>
        /// Gets the navigation CSS class for the Two-Factor Authentication page.
        /// </summary>
        /// <param name="viewContext">The view context.</param>
        /// <returns>"active" if current page is TwoFactorAuthentication, otherwise null.</returns>
        public static string? TwoFactorAuthenticationNavClass(ViewContext viewContext) => PageNavClass(viewContext, TwoFactorAuthentication);

        /// <summary>
        /// Determines if the specified page is the active page.
        /// </summary>
        /// <param name="viewContext">The view context.</param>
        /// <param name="page">The page identifier to check.</param>
        /// <returns>"active" if the page matches the active page, otherwise null.</returns>
        public static string? PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["ActivePage"] as string;
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }

        /// <summary>
        /// Sets the active page identifier in ViewData.
        /// </summary>
        /// <param name="viewData">The ViewData dictionary.</param>
        /// <param name="activePage">The page identifier to set as active.</param>
        public static void AddActivePage(this ViewDataDictionary viewData, string activePage) => viewData[ActivePageKey] = activePage;
    }
}
