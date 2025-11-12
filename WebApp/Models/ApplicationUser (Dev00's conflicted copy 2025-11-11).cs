using Microsoft.AspNetCore.Identity;

namespace WebApp.Models
{
    /// <summary>
    /// Represents an application user with identity authentication.
    /// Extends IdentityUser to support custom profile data if needed.
    /// </summary>
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
    }
}