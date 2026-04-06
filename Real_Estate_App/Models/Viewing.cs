using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Real_Estate_App.Models
{
    public class Viewing
    {
        [Key]
        [BindNever]
        public int Viewing_ID { get; set; }


        [Required]
        public DateTime Viewing_TimeDate { get; set; }

        [Required]
        public string Viewing_Status { get; set; }

        [Required]
        public string? Viewing_Description { get; set; }


        public int UserID { get; set; }

        public User_Data Users_Data { get; set; }


        // Many //
        public int PropertyID { get; set; }

        public Property PropertiesViewed { get; set; }
        // Many //
    }
}
