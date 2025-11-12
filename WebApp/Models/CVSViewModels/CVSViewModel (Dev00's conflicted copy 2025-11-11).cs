namespace WebApp.Models.CVSViewModels
{
    /// <summary>
    /// View model for CVS pharmacy operations page.
    /// Contains status message information for user feedback.
    /// </summary>
    public class CVSViewModel
    {
        /// <summary>
        /// Gets or sets the status message to display to the user.
        /// </summary>
        public string statusMessage { get; set; } = "";
    }
}