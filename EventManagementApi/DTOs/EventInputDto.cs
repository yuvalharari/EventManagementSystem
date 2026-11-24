using System.ComponentModel.DataAnnotations;

namespace EventManagementApi.DTOs
{
    // Shared fields and validation rules for creating and updating an event
    public abstract class EventInputDto : IValidatableObject
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; } = null!;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Range(1, 100000)]
        public int MaxRegistrations { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string Location { get; set; } = null!;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndDate < StartDate)
            {
                yield return new ValidationResult(
                    "End date must be on or after the start date.",
                    new[] { nameof(EndDate) });
            }
        }
    }
}
