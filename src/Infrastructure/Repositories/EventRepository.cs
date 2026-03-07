using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly AppDbContext _db;

        public EventRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Event @event)
        {
            await _db.Events.AddAsync(@event);
            await _db.SaveChangesAsync();
        }

        public async Task<Event?> GetByIdAsync(Guid id)
        {
            return await _db.Events.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<Event>> GetEventsAsync(DateTimeOffset? from = null, DateTimeOffset? to = null)
        {
            var q = _db.Events.AsQueryable();
            if (from.HasValue) q = q.Where(e => e.StartTime >= from.Value);
            if (to.HasValue) q = q.Where(e => e.StartTime <= to.Value);
            return await q.OrderBy(e => e.StartTime).ToListAsync();
        }

        public async Task<int> CountEnrolledAsync(Guid eventId)
        {
            return await _db.Registrations.CountAsync(r => r.EventId == eventId && r.Status == RegistrationStatus.Enrolled);
        }

        public async Task UpdateAsync(Event @event)
        {
            _db.Events.Update(@event);
            await _db.SaveChangesAsync();
        }
    }
}
