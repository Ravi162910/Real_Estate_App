using System.ComponentModel.DataAnnotations;

namespace Real_Estate_App.Models
{
    // Backs the public "Contact Us" support form. Not persisted - submissions
    // are delivered to the support inbox by email, with an automatic
    // acknowledgement sent back to the person who filled it out.
    public class ContactRequestViewModel
    {
        [Required(ErrorMessage = "Please fill out this field")]
        [MaxLength(50, ErrorMessage = "50 characters are allowed for the Name")]
        [Display(Name = "Your Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please fill out this field")]
        [MaxLength(100, ErrorMessage = "100 characters are allowed for the Email")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Your Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please choose what your message is about")]
        [MaxLength(60)]
        [Display(Name = "What can we help with?")]
        public string Problem { get; set; }

        [Required(ErrorMessage = "Please describe the issue")]
        [MaxLength(2000, ErrorMessage = "2000 characters are allowed for the comments")]
        [Display(Name = "Additional Comments")]
        public string Comments { get; set; }

        // The fixed list of problem categories shown in the dropdown. Kept here
        // so the controller can re-validate the submitted value server-side.
        public static readonly string[] ProblemOptions = new[]
        {
            "Account and login",
            "Property listing issue",
            "Viewing or booking",
            "Purchase or payment",
            "Report a bug",
            "Other"
        };
    }
}
