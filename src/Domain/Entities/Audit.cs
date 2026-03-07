using System;

namespace Domain.Entities
{
    public class Audit
    {
        public Guid Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public Guid? EntityId { get; set; }
        public string PerformedBy { get; set; } = string.Empty; // user id or system
        public DateTimeOffset PerformedAt { get; set; }
        public string? Details { get; set; }
    }
}
