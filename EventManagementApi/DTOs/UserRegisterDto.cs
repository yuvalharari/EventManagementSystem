using System.ComponentModel.DataAnnotations;

namespace EventManagementApi.DTOs
{
    public class UserRegisterDto : IValidatableObject
    {
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string Name { get; set; } = null!;

        [Required]
        public DateTime DateOfBirth { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DateOfBirth.Date > DateTime.Today)
            {
                yield return new ValidationResult(
                    "Date of birth cannot be in the future.",
                    new[] { nameof(DateOfBirth) });
            }
        }
    }
}
