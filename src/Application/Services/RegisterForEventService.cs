using System;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services
{
    public class RegisterForEventService
    {
        private readonly IRegistrationRepository _registrations;
        private readonly Application.Interfaces.IAuditRepository _audit;

        public RegisterForEventService(IRegistrationRepository registrations, Application.Interfaces.IAuditRepository audit)
        {
            _registrations = registrations;
            _audit = audit;
        }

        public async Task<(Guid registrationId, RegistrationStatus status)> RegisterAsync(Guid eventId, Guid residentId)
        {
            var reg = new Registration
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                ResidentId = residentId,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                Source = RegistrationSource.Manual
            };

            var status = await _registrations.EnrollOrWaitlistAsync(eventId, reg);

            await _audit.AddAsync(new Domain.Entities.Audit
            {
                Id = Guid.NewGuid(),
                Action = status == RegistrationStatus.Enrolled ? "EnrollResident" : "WaitlistResident",
                EntityName = nameof(Registration),
                EntityId = reg.Id,
                PerformedBy = "system",
                PerformedAt = DateTimeOffset.UtcNow,
                Details = $"EventId={eventId};ResidentId={residentId}"
            });

            return (reg.Id, status);
        }
    }
}
