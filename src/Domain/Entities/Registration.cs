using System;

namespace Domain.Entities
{
    public enum RegistrationStatus { Enrolled = 0, Waitlisted = 1, Cancelled = 2 }
    public enum RegistrationSource { Manual = 0, AutoEnroll = 1 }

    public class Registration
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public Guid ResidentId { get; set; }
        public RegistrationStatus Status { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public int? Position { get; set; }
        public RegistrationSource Source { get; set; } = RegistrationSource.Manual;
    }
}
