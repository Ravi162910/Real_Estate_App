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

        // Cosmetic payment fields - validated for format only, never stored
        [Required(ErrorMessage = "Card number is required")]
        [Display(Name = "Card Number")]
        [RegularExpression(@"^\d{4}\s?\d{4}\s?\d{4}\s?\d{4}$", ErrorMessage = "Please enter a valid 16-digit card number")]
        public string CardNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cardholder name is required")]
        [Display(Name = "Name on Card")]
        [StringLength(100)]
        public string CardholderName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Expiry date is required")]
        [Display(Name = "Expiry Date")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "Please enter a valid expiry date (MM/YY)")]
        public string ExpiryDate { get; set; } = string.Empty;

        [Required(ErrorMessage = "CVV is required")]
        [Display(Name = "CVV")]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "Please enter a valid 3 or 4 digit CVV")]
        public string Cvv { get; set; } = string.Empty;
    }
}
