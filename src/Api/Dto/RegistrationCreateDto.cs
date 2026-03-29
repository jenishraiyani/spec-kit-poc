using System;
using System.ComponentModel.DataAnnotations;

namespace Api.Dto
{
    public class RegistrationCreateDto : IValidatableObject
    {
        [Required]
        public Guid ResidentId { get; set; }

        public System.Collections.Generic.IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ResidentId == Guid.Empty)
                yield return new ValidationResult("ResidentId must not be an empty GUID.", new[] { nameof(ResidentId) });
        }
    }
}
