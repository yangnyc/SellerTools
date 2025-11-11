namespace WebApp.Models.DemoViewModels
{
    /// <summary>
    /// View model for demo/testing operations page.
    /// Contains status message information for user feedback.
    /// </summary>
    public class DemoViewModel
    {
        /// <summary>
        /// Gets or sets the status message to display to the user.
        /// </summary>
        public string statusMessage { get; set; } = "";
    }
}