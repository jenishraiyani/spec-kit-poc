using System;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services
{
    public class CancelEventService
    {
        private readonly IEventRepository _events;
        private readonly IAuditRepository _audit;

        public CancelEventService(IEventRepository events, IAuditRepository audit)
        {
            _events = events;
            _audit = audit;
        }

        public async Task CancelAsync(Guid eventId, string performedBy = "system")
        {
            var ev = await _events.GetByIdAsync(eventId);
            if (ev == null) throw new InvalidOperationException("Event not found");
            ev.Status = EventStatus.Cancelled;
            await _events.UpdateAsync(ev);

            await _audit.AddAsync(new Audit
            {
                Id = Guid.NewGuid(),
                Action = "CancelEvent",
                EntityName = nameof(Event),
                EntityId = ev.Id,
                PerformedBy = performedBy,
                PerformedAt = DateTimeOffset.UtcNow,
                Details = $"Title={ev.Title}"
            });
        }
    }
}
