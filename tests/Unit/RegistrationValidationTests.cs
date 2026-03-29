using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Api.Dto;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Api.Controllers;
using Xunit;

namespace Unit.Tests
{
    public class RegistrationValidationTests
    {
        // ── DTO validation helpers ────────────────────────────────────────────

        private static IList<ValidationResult> ValidateDto(object dto)
        {
            var results = new List<ValidationResult>();
            var ctx = new ValidationContext(dto);
            Validator.TryValidateObject(dto, ctx, results, validateAllProperties: true);
            return results;
        }

        // ── RegistrationCreateDto ─────────────────────────────────────────────

        [Fact]
        public void RegistrationCreateDto_EmptyGuid_FailsValidation()
        {
            var dto = new RegistrationCreateDto { ResidentId = Guid.Empty };
            var errors = ValidateDto(dto);
            Assert.Contains(errors, e => e.MemberNames != null &&
                System.Linq.Enumerable.Contains(e.MemberNames, nameof(RegistrationCreateDto.ResidentId)));
        }

        [Fact]
        public void RegistrationCreateDto_ValidGuid_PassesValidation()
        {
            var dto = new RegistrationCreateDto { ResidentId = Guid.NewGuid() };
            var errors = ValidateDto(dto);
            Assert.Empty(errors);
        }

        // ── EventCreateDto ────────────────────────────────────────────────────

        [Fact]
        public void EventCreateDto_EndBeforeStart_FailsValidation()
        {
            var now = DateTimeOffset.UtcNow;
            var dto = new EventCreateDto
            {
                Title = "Test",
                StartTime = now.AddHours(2),
                EndTime = now.AddHours(1),
                Capacity = 10
            };
            var errors = ValidateDto(dto);
            Assert.Contains(errors, e => e.MemberNames != null &&
                System.Linq.Enumerable.Contains(e.MemberNames, nameof(EventCreateDto.EndTime)));
        }

        [Fact]
        public void EventCreateDto_ZeroCapacity_FailsValidation()
        {
            var now = DateTimeOffset.UtcNow;
            var dto = new EventCreateDto
            {
                Title = "Test",
                StartTime = now.AddHours(1),
                EndTime = now.AddHours(2),
                Capacity = 0
            };
            var errors = ValidateDto(dto);
            Assert.Contains(errors, e => e.MemberNames != null &&
                System.Linq.Enumerable.Contains(e.MemberNames, nameof(EventCreateDto.Capacity)));
        }

        [Fact]
        public void EventCreateDto_MissingTitle_FailsValidation()
        {
            var now = DateTimeOffset.UtcNow;
            var dto = new EventCreateDto
            {
                Title = string.Empty,
                StartTime = now.AddHours(1),
                EndTime = now.AddHours(2),
                Capacity = 10
            };
            var errors = ValidateDto(dto);
            Assert.Contains(errors, e => e.MemberNames != null &&
                System.Linq.Enumerable.Contains(e.MemberNames, nameof(EventCreateDto.Title)));
        }

        [Fact]
        public void EventCreateDto_ValidData_PassesValidation()
        {
            var now = DateTimeOffset.UtcNow;
            var dto = new EventCreateDto
            {
                Title = "Community Meetup",
                StartTime = now.AddDays(1),
                EndTime = now.AddDays(1).AddHours(2),
                Capacity = 50
            };
            var errors = ValidateDto(dto);
            Assert.Empty(errors);
        }

        // ── Controller returns 400 for invalid RegistrationCreateDto ─────────

        private class FakeRegRepo : IRegistrationRepository
        {
            public Task AddAsync(Registration r) => Task.CompletedTask;
            public Task<Registration?> GetByIdAsync(Guid id) => Task.FromResult<Registration?>(null);
            public Task<bool> ExistsActiveRegistrationAsync(Guid eventId, Guid residentId) => Task.FromResult(false);
            public Task<List<Registration>> GetWaitlistAsync(Guid eventId, int limit = 50) => Task.FromResult(new List<Registration>());
            public Task UpdateAsync(Registration r) => Task.CompletedTask;
            public Task<RegistrationStatus> EnrollOrWaitlistAsync(Guid eventId, Registration r) => Task.FromResult(RegistrationStatus.Enrolled);
        }

        private class FakeAuditRepo : IAuditRepository
        {
            public Task AddAsync(Audit a) => Task.CompletedTask;
        }

        [Fact]
        public async Task Register_InvalidModelState_Returns400()
        {
            var svc = new RegisterForEventService(new FakeRegRepo(), new FakeAuditRepo());
            var controller = new RegistrationsController(svc);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            // Simulate a model-state error (e.g. empty ResidentId)
            controller.ModelState.AddModelError("ResidentId", "ResidentId must not be an empty GUID.");

            var result = await controller.Register(Guid.NewGuid(), new RegistrationCreateDto { ResidentId = Guid.Empty });

            // ValidationProblem returns ObjectResult containing ValidationProblemDetails
            var objectResult = Assert.IsType<ObjectResult>(result);
            var problem = Assert.IsType<Microsoft.AspNetCore.Mvc.ValidationProblemDetails>(objectResult.Value);
            Assert.True(problem.Errors.ContainsKey("ResidentId"));
        }

        // ── Service propagates ArgumentException from repo ───────────────────

        private class CancelledEventRepo : IRegistrationRepository
        {
            public Task AddAsync(Registration r) => Task.CompletedTask;
            public Task<Registration?> GetByIdAsync(Guid id) => Task.FromResult<Registration?>(null);
            public Task<bool> ExistsActiveRegistrationAsync(Guid eventId, Guid residentId) => Task.FromResult(false);
            public Task<List<Registration>> GetWaitlistAsync(Guid eventId, int limit = 50) => Task.FromResult(new List<Registration>());
            public Task UpdateAsync(Registration r) => Task.CompletedTask;
            public Task<RegistrationStatus> EnrollOrWaitlistAsync(Guid eventId, Registration r)
                => throw new ArgumentException("Registration is not allowed for a cancelled event.", "eventId");
        }

        [Fact]
        public async Task RegisterForEvent_CancelledEvent_ThrowsArgumentException()
        {
            var svc = new RegisterForEventService(new CancelledEventRepo(), new FakeAuditRepo());
            await Assert.ThrowsAsync<ArgumentException>(() => svc.RegisterAsync(Guid.NewGuid(), Guid.NewGuid()));
        }

        private class ClosedWindowRepo : IRegistrationRepository
        {
            public Task AddAsync(Registration r) => Task.CompletedTask;
            public Task<Registration?> GetByIdAsync(Guid id) => Task.FromResult<Registration?>(null);
            public Task<bool> ExistsActiveRegistrationAsync(Guid eventId, Guid residentId) => Task.FromResult(false);
            public Task<List<Registration>> GetWaitlistAsync(Guid eventId, int limit = 50) => Task.FromResult(new List<Registration>());
            public Task UpdateAsync(Registration r) => Task.CompletedTask;
            public Task<RegistrationStatus> EnrollOrWaitlistAsync(Guid eventId, Registration r)
                => throw new ArgumentException("Registration for this event has closed.", "eventId");
        }

        [Fact]
        public async Task RegisterForEvent_ClosedRegistrationWindow_ThrowsArgumentException()
        {
            var svc = new RegisterForEventService(new ClosedWindowRepo(), new FakeAuditRepo());
            await Assert.ThrowsAsync<ArgumentException>(() => svc.RegisterAsync(Guid.NewGuid(), Guid.NewGuid()));
        }
    }
}
