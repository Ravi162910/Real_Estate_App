using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Real_Estate_App.Models
{
    public class Viewing
    {
        [Key]
        public int Viewing_ID { get; set; }


        [Required]
        [DisplayName("Date/Time for Viewing")]
        public DateTime Viewing_TimeDate { get; set; }


        // Many //
        public int UserID { get; set; }

        public User_Data? Users { get; set; }
        // Many //



        // Many //
        public int PropertyID { get; set; }

        public Property? Properties { get; set; }
        // Many //
    }
}
