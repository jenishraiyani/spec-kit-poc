using System;

namespace Domain.Entities
{
    public enum EventStatus { Scheduled = 0, Cancelled = 1 }

    public class Event
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public int Capacity { get; set; }
        public DateTimeOffset? RegistrationOpen { get; set; }
        public DateTimeOffset? RegistrationClose { get; set; }
        public EventStatus Status { get; set; } = EventStatus.Scheduled;
        public string Timezone { get; set; } = "UTC";
    }
}
