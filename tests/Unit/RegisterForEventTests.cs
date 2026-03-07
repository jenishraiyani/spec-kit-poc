using System;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Xunit;

namespace Unit.Tests
{
    public class RegisterForEventTests
    {
        private class FakeRegistrationRepo : IRegistrationRepository
        {
            public RegistrationStatus ReturnStatus { get; set; } = RegistrationStatus.Enrolled;
            public Task AddAsync(Registration registration) => Task.CompletedTask;
            public Task<Registration?> GetByIdAsync(Guid id) => Task.FromResult<Registration?>(null);
            public Task<bool> ExistsActiveRegistrationAsync(Guid eventId, Guid residentId) => Task.FromResult(false);
            public Task<System.Collections.Generic.List<Registration>> GetWaitlistAsync(Guid eventId, int limit = 50) => Task.FromResult(new System.Collections.Generic.List<Registration>());
            public Task UpdateAsync(Registration registration) => Task.CompletedTask;
            public Task<RegistrationStatus> EnrollOrWaitlistAsync(Guid eventId, Registration registration) => Task.FromResult(ReturnStatus);
        }

        [Fact]
        public async Task RegisterForEvent_Returns_Enrolled_Status()
        {
            var repo = new FakeRegistrationRepo { ReturnStatus = RegistrationStatus.Enrolled };
            var svc = new RegisterForEventService(repo);
            var (id, status) = await svc.RegisterAsync(Guid.NewGuid(), Guid.NewGuid());
            Assert.NotEqual(Guid.Empty, id);
            Assert.Equal(RegistrationStatus.Enrolled, status);
        }
    }
}
