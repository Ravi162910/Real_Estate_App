using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Real_Estate_App.Models
{
    public class LoginViewModel
    {

        [Required(ErrorMessage = "Please fill out this field")]
        [MaxLength(25, ErrorMessage = "25 characters are allowed for the UserName/Email")]
        [DisplayName("Email or Username")]
        public string UserNameorEmail { get; set; }

        [Required(ErrorMessage = "Please fill out this field")]
        [MaxLength(25, ErrorMessage = "25 characters are allowed for the Password")]
        public string Password { get; set; }
    }
}
