using System.ComponentModel.DataAnnotations;

namespace Real_Estate_App.Models
{
    public class Property
    {
        [Key]
        public int PropertyId { get; set; }

        [Required(ErrorMessage = "Property name is required")]
        [StringLength(100)]
        [Display(Name = "Property Name")]
        public string PropertyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200)]
        [Display(Name = "Address")]
        public string PropertyAddress { get; set; } = string.Empty;

        [Required]
        [Range(0, 20, ErrorMessage = "Bedrooms must be between 0 and 20")]
        [Display(Name = "Bedrooms")]
        public int PropertyBedrooms { get; set; }

        [Required]
        [Range(0, 20, ErrorMessage = "Bathrooms must be between 0 and 20")]
        [Display(Name = "Bathrooms")]
        public int PropertyBathrooms { get; set; }

        [Required]
        [Range(0, 10, ErrorMessage = "Pets allowed must be between 0 and 10")]
        [Display(Name = "Pets Allowed")]
        public int PropertyPets { get; set; }

        [Required]
        [Range(0, 10, ErrorMessage = "Garages must be between 0 and 10")]
        [Display(Name = "Garages")]
        public int PropertyGarages { get; set; }

        [StringLength(2000)]
        [Display(Name = "Description")]
        public string? ExtendedDescription { get; set; }

        [Required]
        [Range(0.01, 99999999.99, ErrorMessage = "Price must be a positive value")]
        [DataType(DataType.Currency)]
        [Display(Name = "Price")]
        public decimal Price { get; set; }

        [Required]
        [Display(Name = "Property Type")]
        public string PropertyType { get; set; } = string.Empty;

        [Display(Name = "Is Available")]
        public bool IsAvailable { get; set; } = true;

        [Display(Name = "Nearby Schools")]
        public string? NearbySchools { get; set; }

        [Display(Name = "Nearby Shops")]
        public string? NearbyShops { get; set; }
    }
}
