using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Real_Estate_App.Models
{       
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(UserName), IsUnique = true)]
    public class User_Data
    {
        [Key]
        [BindNever]
        public int? UserID { get; set; }

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

        [Required]
        [MaxLength(255)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool IsAdmin { get; set; }

        // "Super" (full access), "Property" (manages properties + viewings),
        // "Transaction" (reviews purchase queue). Null/empty for non-admins.
        // Why: tutor wants role-separated admins so a transaction reviewer
        // can't also edit listings.
        [MaxLength(20)]
        [Display(Name = "Admin Role")]
        public string? AdminRole { get; set; }


        public List<Viewing>? Viewings_list { get; set; }// One
    }
}