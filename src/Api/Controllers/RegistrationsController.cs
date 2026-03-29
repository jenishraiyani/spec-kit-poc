using System;
using System.Threading.Tasks;
using Api.Dto;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/events/{eventId:guid}/registrations")]
    public class RegistrationsController : ControllerBase
    {
        private readonly RegisterForEventService _registerService;

        public RegistrationsController(RegisterForEventService registerService)
        {
            _registerService = registerService;
        }

        [HttpPost]
        public async Task<IActionResult> Register(Guid eventId, [FromBody] RegistrationCreateDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                var (id, status) = await _registerService.RegisterAsync(eventId, dto.ResidentId);
                if (status == Domain.Entities.RegistrationStatus.Enrolled)
                    return CreatedAtAction(null, new { id }, new { id });
                else
                    return Accepted(new { id, status = "Waitlisted" });
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(ex.ParamName ?? string.Empty, ex.Message);
                return ValidationProblem(ModelState);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpDelete("{registrationId:guid}")]
        public async Task<IActionResult> CancelRegistration(Guid eventId, Guid registrationId)
        {
            try
            {
                var repo = (Application.Interfaces.IRegistrationRepository)HttpContext.RequestServices.GetService(typeof(Application.Interfaces.IRegistrationRepository))!;
                var reg = await repo.GetByIdAsync(registrationId);
                if (reg == null || reg.EventId != eventId) return NotFound();
                reg.Status = Domain.Entities.RegistrationStatus.Cancelled;
                reg.UpdatedAt = DateTimeOffset.UtcNow;
                await repo.UpdateAsync(reg);

                var audit = (Application.Interfaces.IAuditRepository)HttpContext.RequestServices.GetService(typeof(Application.Interfaces.IAuditRepository))!;
                await audit.AddAsync(new Domain.Entities.Audit
                {
                    Id = Guid.NewGuid(),
                    Action = "CancelRegistration",
                    EntityName = nameof(Domain.Entities.Registration),
                    EntityId = reg.Id,
                    PerformedBy = "system",
                    PerformedAt = DateTimeOffset.UtcNow,
                    Details = $"EventId={eventId};RegistrationId={registrationId}"
                });

                // Promote first waitlisted registration (FIFO) when a spot frees
                var waitlist = await repo.GetWaitlistAsync(eventId, 1);
                if (waitlist != null && waitlist.Count > 0)
                {
                    var promote = waitlist[0];
                    promote.Status = Domain.Entities.RegistrationStatus.Enrolled;
                    promote.Position = null;
                    promote.UpdatedAt = DateTimeOffset.UtcNow;
                    await repo.UpdateAsync(promote);

                    await audit.AddAsync(new Domain.Entities.Audit
                    {
                        Id = Guid.NewGuid(),
                        Action = "PromoteWaitlist",
                        EntityName = nameof(Domain.Entities.Registration),
                        EntityId = promote.Id,
                        PerformedBy = "system",
                        PerformedAt = DateTimeOffset.UtcNow,
                        Details = $"EventId={eventId};PromotedRegistrationId={promote.Id}"
                    });
                }

                return NoContent();
            }
            catch (InvalidOperationException)
            {
                return Conflict();
            }
        }
    }
}
