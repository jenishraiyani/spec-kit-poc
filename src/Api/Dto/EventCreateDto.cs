using System;
using System.ComponentModel.DataAnnotations;

namespace Api.Dto
{
    public class EventCreateDto : IValidatableObject
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? Location { get; set; }

        public DateTimeOffset StartTime { get; set; }

        public DateTimeOffset EndTime { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1.")]
        public int Capacity { get; set; }

        public DateTimeOffset? RegistrationOpen { get; set; }
        public DateTimeOffset? RegistrationClose { get; set; }
        public string? Timezone { get; set; }

        public System.Collections.Generic.IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndTime <= StartTime)
                yield return new ValidationResult("EndTime must be after StartTime.", new[] { nameof(EndTime) });

            if (RegistrationClose.HasValue && RegistrationOpen.HasValue && RegistrationClose <= RegistrationOpen)
                yield return new ValidationResult("RegistrationClose must be after RegistrationOpen.", new[] { nameof(RegistrationClose) });
        }
    }
}
