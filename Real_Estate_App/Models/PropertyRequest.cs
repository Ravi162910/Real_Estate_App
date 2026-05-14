using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Real_Estate_App.Models
{
    public class PropertyRequest
    {
        public int PropertyRequestID { get; set; }

        public int UserID { get; set; }

        public User_Data User { get; set; }

        [DisplayName("Property Name")]
        [Required(ErrorMessage = "Property name is required")]
        [StringLength(100)]
        public string Property_Name { get; set; }

        [DisplayName("Property Description")]
        [StringLength(2000)]
        public string Property_Description { get; set; }

        [DisplayName("Property Price")]
        [Required]
        [Range(0.01, 99999999.99, ErrorMessage = "Price must be a positive value")]
        [DataType(DataType.Currency)]
        public decimal Property_Price { get; set; }

        [DisplayName("Property Address")]
        [Required(ErrorMessage = "Address is required")]
        [StringLength(200)]
        public string Property_Address { get; set; }

        [DisplayName("Property Type")]
        public string Property_Type { get; set; }

        [DisplayName("Nearby Schools")]
        public string Request_NearbySchools { get; set; }

        [DisplayName("Nearby Shops")]
        public string Request_NearbyShops { get; set; }

        [Required]
        [Range(0, 20, ErrorMessage = "Bedrooms must be between 0 and 20")]
        [Display(Name = "Bedrooms")]
        public int Property_Bedrooms { get; set; }

        [Required]
        [Range(0, 20, ErrorMessage = "Bathrooms must be between 0 and 20")]
        [Display(Name = "Bathrooms")]
        public int Property_Bathrooms { get; set; }

        [Required]
        [Range(0, 10, ErrorMessage = "Pets allowed must be between 0 and 10")]
        [Display(Name = "Pets")]
        public int Property_Pets { get; set; }

        [Required]
        [Range(0, 10, ErrorMessage = "Garages must be between 0 and 10")]
        [Display(Name = "Garages")]
        public int Property_Garages { get; set; }

        public string Requeststatus { get; set; }

        public DateTime Requestcreatedat { get; set; }

        [Display(Name = "Image URL for Property Photo")]
        [Required(ErrorMessage = "A Property photo is required.")]
        public string? ImageUrl { get; set; }
    }
}
