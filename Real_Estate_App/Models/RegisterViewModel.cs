using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Real_Estate_App.Models
{
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(UserName), IsUnique = true)]
    public class RegisterViewModel
    {
        [Key]
        [BindNever]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please fill out this field")]
        [MaxLength(25, ErrorMessage = "25 characters are allowed for the First Name")]
        public string First_Name { get; set; }

        [Required(ErrorMessage = "Please fill out this field")]
        [MaxLength(25, ErrorMessage = "25 characters are allowed for the Last Name")]
        public string Last_Name { get; set; }

        [Required(ErrorMessage = "Please fill out this field")]
        [MaxLength(100, ErrorMessage = "100 characters are allowed for the Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please fill out this field")]
        [MaxLength(25, ErrorMessage = "25 characters are allowed for the UserName")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Please fill out this field")]
        [MaxLength(25, ErrorMessage = "25 characters are allowed for the Password")]
        public string Password { get; set; }
        
        [Required(ErrorMessage = "Please fill out this field")]
        [MaxLength(25, ErrorMessage = "25 characters are allowed for this field")]
        [Compare("Password", ErrorMessage = "Both passwords must match")]
        public string ConfirmPassword { get; set; }
    }
}
