using System;

namespace WebApp.Models
{
    /// <summary>
    /// View model for displaying error information.
    /// Used by the error handling middleware to show error details.
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// Gets or sets the unique request identifier for tracking errors.
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// Gets a value indicating whether the request ID should be displayed.
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
