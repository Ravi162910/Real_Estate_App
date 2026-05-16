using System.ComponentModel.DataAnnotations;

namespace Real_Estate_App.Models
{
    public class CheckoutViewModel
    {
        public int PropertyId { get; set; }
        public string PropertyName { get; set; } = string.Empty;
        public string PropertyAddress { get; set; } = string.Empty;
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [Display(Name = "Full Name")]
        [StringLength(100)]
        public string BuyerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Email Address")]
        public string UserEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Billing address is required")]
        [Display(Name = "Billing Address")]
        [StringLength(200)]
        public string BillingAddress { get; set; } = string.Empty;

        // Card data is intentionally NOT collected here. Payment is handled
        // entirely on Stripe's hosted Checkout page, so card numbers / CVV
        // never touch this application or its database (PCI scope stays low).
    }
}
