using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.AccountViewModels
{
    public class LoginViewModel
    {
        [Display(Name = "Email or Username")]
        public required string UserName { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }
}
