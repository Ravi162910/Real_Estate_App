using System.ComponentModel.DataAnnotations;

namespace Real_Estate_App.Models
{
    public class OpenHome : IValidatableObject
    {
        public int OpenHomeId { get; set; }

        public int PropertyId { get; set; }

        public Property? Property { get; set; }//navigation property...

        [Required]
        [Display(Name = "Start day/time for Open Home")]
        public DateTime StartTime { get; set; }

        [Required]
        [Display(Name = "End day/time for Open Home")]
        public DateTime EndTime { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndTime <= StartTime)
            {
                yield return new ValidationResult(
                    "End time must be after the start time.",
                    new[] { nameof(EndTime) });
            }
        }
    }
}
