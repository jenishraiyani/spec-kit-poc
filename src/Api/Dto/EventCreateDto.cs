using System;

namespace Api.Dto
{
    public class EventCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public int Capacity { get; set; }
        public DateTimeOffset? RegistrationOpen { get; set; }
        public DateTimeOffset? RegistrationClose { get; set; }
        public string? Timezone { get; set; }
    }
}
