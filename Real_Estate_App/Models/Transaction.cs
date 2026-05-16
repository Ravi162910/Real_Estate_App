using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Real_Estate_App.Models
{
    public class Transaction
    {
        public const string StatusPending = "Pending";
        public const string StatusApproved = "Approved";
        public const string StatusRejected = "Rejected";

        [Key]
        public int TransactionId { get; set; }

        [Required]
        [Display(Name = "Property")]
        public int PropertyId { get; set; }

        [Required]
        [Display(Name = "User")]
        public int UserId { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        [Display(Name = "Price")]
        public decimal Price { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string UserEmail { get; set; } = string.Empty;

        [Display(Name = "Buyer Name")]
        public string BuyerName { get; set; } = string.Empty;

        [Display(Name = "Purchase Date")]
        [DataType(DataType.DateTime)]
        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(20)]
        [Display(Name = "Status")]
        public string Status { get; set; } = StatusPending;

        [Display(Name = "Reviewed By")]
        public int? ReviewedByUserId { get; set; }

        [Display(Name = "Reviewed Date")]
        [DataType(DataType.DateTime)]
        public DateTime? ReviewedDate { get; set; }

        [MaxLength(500)]
        [Display(Name = "Rejection Reason")]
        public string? RejectionReason { get; set; }

        // Stripe Checkout Session id ("cs_test_...") that authorized this row.
        // Used to make fulfilment idempotent: the success redirect and the
        // webhook can both arrive, but only one Transaction is created.
        [MaxLength(255)]
        [Display(Name = "Stripe Session")]
        public string? StripeSessionId { get; set; }

        // PaymentIntent ("pi_...") behind the session. The buyer's card is
        // authorized (held) against this; admin Approve captures it, admin
        // Reject cancels it. Null for legacy/non-Stripe rows.
        [MaxLength(255)]
        [Display(Name = "Stripe Payment")]
        public string? StripePaymentIntentId { get; set; }

        [ForeignKey("PropertyId")]
        public Property? Property { get; set; }
    }
}
