using System;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Application.Services;
using Xunit;
using Api.Controllers;

namespace Unit.Tests
{
    public class CancelRegistrationTests
    {
        private class FakeRegistrationRepo : IRegistrationRepository
        {
            public Registration? LastUpdated { get; private set; }
            public Registration? ToReturn { get; set; }
            public Task AddAsync(Registration registration) => Task.CompletedTask;
            public Task<Registration?> GetByIdAsync(Guid id) => Task.FromResult(ToReturn);
            public Task<bool> ExistsActiveRegistrationAsync(Guid eventId, Guid residentId) => Task.FromResult(false);
            public Task<System.Collections.Generic.List<Registration>> GetWaitlistAsync(Guid eventId, int limit = 50) => Task.FromResult(new System.Collections.Generic.List<Registration>());
            public Task UpdateAsync(Registration registration)
            {
                LastUpdated = registration;
                return Task.CompletedTask;
            }
            public Task<RegistrationStatus> EnrollOrWaitlistAsync(Guid eventId, Registration registration) => Task.FromResult(RegistrationStatus.Enrolled);
        }

        private class FakeAuditRepo : IAuditRepository
        {
            public bool Added { get; private set; }
            public Task AddAsync(Audit audit)
            {
                Added = true;
                return Task.CompletedTask;
            }
        }

        private class SimpleProvider : IServiceProvider
        {
            private readonly IRegistrationRepository _reg;
            private readonly IAuditRepository _audit;
            public SimpleProvider(IRegistrationRepository reg, IAuditRepository audit) { _reg = reg; _audit = audit; }
            public object? GetService(Type serviceType)
            {
                if (serviceType == typeof(IRegistrationRepository)) return _reg;
                if (serviceType == typeof(IAuditRepository)) return _audit;
                return null;
            }
        }

        [Fact]
        public async Task CancelRegistration_Controller_Marks_Cancelled_And_Writes_Audit()
        {
            var regId = Guid.NewGuid();
            var eventId = Guid.NewGuid();
            var repo = new FakeRegistrationRepo();
            repo.ToReturn = new Registration { Id = regId, EventId = eventId, Status = RegistrationStatus.Enrolled };
            var audit = new FakeAuditRepo();

            var svc = new RegisterForEventService(repo, audit);
            var controller = new RegistrationsController(svc);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { RequestServices = new SimpleProvider(repo, audit) };

            var result = await controller.CancelRegistration(eventId, regId);

            Assert.IsType<NoContentResult>(result);
            Assert.NotNull(repo.LastUpdated);
            Assert.Equal(RegistrationStatus.Cancelled, repo.LastUpdated!.Status);
            Assert.True(audit.Added);
        }
    }
}
