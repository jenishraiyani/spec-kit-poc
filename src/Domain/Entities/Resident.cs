using System;

namespace Domain.Entities
{
    public class Resident
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Timezone { get; set; }
    }
}
