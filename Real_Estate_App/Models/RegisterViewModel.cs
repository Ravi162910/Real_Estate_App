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
        [Display(Name ="First Name")]
        public string First_Name { get; set; }

        [Required(ErrorMessage = "Please fill out this field")]
        [MaxLength(25, ErrorMessage = "25 characters are allowed for the Last Name")]
        [Display(Name = "Last Name")]
        public string Last_Name { get; set; }

        [Required(ErrorMessage = "Please fill out this field")]
        [MaxLength(100, ErrorMessage = "100 characters are allowed for the Email")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please fill out this field")]
        [MaxLength(25, ErrorMessage = "25 characters are allowed for the UserName")]
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Please fill out this field")]
        [MaxLength(25, ErrorMessage = "25 characters are allowed for the Password")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please fill out this field")]
        [MaxLength(25, ErrorMessage = "25 characters are allowed for this field")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Both passwords must match")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
    }
}
