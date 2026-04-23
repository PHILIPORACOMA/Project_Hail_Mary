using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
namespace Project_Hail_Mary.Models
{
    public class Users 
    {
        [Required(ErrorMessage = "Name is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(30, MinimumLength = 8)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }
}
