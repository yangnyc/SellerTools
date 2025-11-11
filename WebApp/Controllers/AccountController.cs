using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using WebApp.Models.AccountViewModels;

namespace WebApp.Controllers
{
    /// <summary>
    /// Handles user account operations including login, registration, and logout.
    /// Manages authentication and authorization for application users.
    /// </summary>
    [Authorize]
    [Route("[controller]/[action]")]
    public class AccountController(UserManager<ApplicationUser> _userManager, SignInManager<ApplicationUser> _signInManager) : Controller
    {
        /// <summary>
        /// Displays the login page.
        /// Clears any existing external authentication cookies.
        /// </summary>
        /// <returns>The login view.</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login()
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            return View();
        }

        /// <summary>
        /// Processes user login attempt.
        /// Validates credentials and signs in the user if successful.
        /// </summary>
        /// <param name="input">Login credentials from the form.</param>
        /// <returns>Redirects to home on success, or returns login view with errors.</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel input)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(input.UserName);
                if (user == null)
                {
                    user = await _userManager.FindByEmailAsync(input.UserName);
                }

                if (user != null && user.UserName != null && await _userManager.CheckPasswordAsync(user, input.Password))
                {
                    var result = await _signInManager.PasswordSignInAsync(user.UserName, input.Password, input.RememberMe, false);

                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
            }
            // If execution got this far, something failed, redisplay the form.
            return View(input);
        }

        /// <summary>
        /// Displays the user registration page.
        /// </summary>
        /// <returns>The registration view.</returns>
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// Processes new user registration.
        /// Creates a new user account and signs them in if successful.
        /// </summary>
        /// <param name="model">Registration data from the form.</param>
        /// <returns>Redirects to home on success, or returns registration view with errors.</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("index", "Home");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            // If execution got this far, something failed, redisplay the form.
            return View(model);
        }

        /// <summary>
        /// Signs out the current user.
        /// </summary>
        /// <returns>Redirects to home page after logout.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        /// <summary>
        /// Displays the access denied page when user lacks permissions.
        /// </summary>
        /// <returns>The access denied view.</returns>
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        /// <summary>
        /// Displays the account lockout page.
        /// </summary>
        /// <returns>The lockout view.</returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            return View();
        }

        /// <summary>
        /// Confirms a user's email address using a confirmation code.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="code">The email confirmation code.</param>
        /// <returns>Confirmation view on success, or error view on failure.</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{userId}'.");
            }
            var result = await _userManager.ConfirmEmailAsync(user, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        /// <summary>
        /// Displays the forgot password page.
        /// </summary>
        /// <returns>The forgot password view.</returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        /// <summary>
        /// Displays the forgot password confirmation page.
        /// </summary>
        /// <returns>The forgot password confirmation view.</returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }
    }
}