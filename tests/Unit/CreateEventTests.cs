using System;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Infrastructure.Repositories;
using Xunit;

namespace Unit.Tests
{
    public class CreateEventTests
    {
        private class FakeEventRepository : IEventRepository
        {
            public Event? LastAdded { get; private set; }
            public Task AddAsync(Event @event)
            {
                LastAdded = @event;
                return Task.CompletedTask;
            }
            public Task<Event?> GetByIdAsync(Guid id) => Task.FromResult<Event?>(null);
            public Task<int> CountEnrolledAsync(Guid eventId) => Task.FromResult(0);
            public Task SystemNotUsed() => Task.CompletedTask;
            public Task<System.Collections.Generic.List<Event>> GetEventsAsync(DateTimeOffset? from = null, DateTimeOffset? to = null) => Task.FromResult(new System.Collections.Generic.List<Event>());
            public Task UpdateAsync(Event @event) => Task.CompletedTask;
        }

        [Fact]
        public async Task CreateEvent_Service_Adds_New_Event()
        {
            var repo = new FakeEventRepository();
            var svc = new CreateEventService(repo);
            var id = await svc.CreateEventAsync("Title", "Desc", "Loc", DateTimeOffset.UtcNow.AddDays(1), DateTimeOffset.UtcNow.AddDays(1).AddHours(2), 10, null, null, "UTC");
            Assert.NotEqual(Guid.Empty, id);
            Assert.NotNull(repo.LastAdded);
            Assert.Equal("Title", repo.LastAdded!.Title);
        }
    }
}
