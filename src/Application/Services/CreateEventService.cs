using System;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services
{
    public class CreateEventService : ICreateEventService
    {
        private readonly IEventRepository _events;
        private readonly Application.Interfaces.IAuditRepository _audit;

        public CreateEventService(IEventRepository events, Application.Interfaces.IAuditRepository audit)
        {
            _events = events;
            _audit = audit;
        }

        public async Task<Guid> CreateEventAsync(string title, string? description, string? location, DateTimeOffset startTime, DateTimeOffset endTime, int capacity, DateTimeOffset? registrationOpen, DateTimeOffset? registrationClose, string timezone)
        {
            var e = new Event
            {
                Id = Guid.NewGuid(),
                Title = title,
                Description = description,
                Location = location,
                StartTime = startTime,
                EndTime = endTime,
                Capacity = capacity,
                RegistrationOpen = registrationOpen,
                RegistrationClose = registrationClose,
                Timezone = timezone ?? "UTC",
                Status = EventStatus.Scheduled
            };

            await _events.AddAsync(e);

            await _audit.AddAsync(new Audit
            {
                Id = Guid.NewGuid(),
                Action = "CreateEvent",
                EntityName = nameof(Event),
                EntityId = e.Id,
                PerformedBy = "system",
                PerformedAt = DateTimeOffset.UtcNow,
                Details = $"Title={e.Title};Capacity={e.Capacity}"
            });

            return e.Id;
        }
    }
}
