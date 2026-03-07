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
    public class RegistrationRepository : IRegistrationRepository
    {
        private readonly AppDbContext _db;

        public RegistrationRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Registration registration)
        {
            await _db.Registrations.AddAsync(registration);
            await _db.SaveChangesAsync();
        }

        public async Task<Registration?> GetByIdAsync(Guid id)
        {
            return await _db.Registrations.FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<bool> ExistsActiveRegistrationAsync(Guid eventId, Guid residentId)
        {
            return await _db.Registrations.AnyAsync(r => r.EventId == eventId && r.ResidentId == residentId && r.Status != RegistrationStatus.Cancelled);
        }

        public async Task<List<Registration>> GetWaitlistAsync(Guid eventId, int limit = 50)
        {
            return await _db.Registrations
                .Where(r => r.EventId == eventId && r.Status == RegistrationStatus.Waitlisted)
                .OrderBy(r => r.Position).ThenBy(r => r.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task UpdateAsync(Registration registration)
        {
            _db.Registrations.Update(registration);
            await _db.SaveChangesAsync();
        }

        public async Task<RegistrationStatus> EnrollOrWaitlistAsync(Guid eventId, Registration registration)
        {
            await using var tx = await _db.Database.BeginTransactionAsync();

            // Check duplicate active registration
            var exists = await _db.Registrations.AnyAsync(r => r.EventId == eventId && r.ResidentId == registration.ResidentId && r.Status != RegistrationStatus.Cancelled);
            if (exists) throw new InvalidOperationException("Duplicate active registration");

            var ev = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId);
            if (ev == null) throw new InvalidOperationException("Event not found");

            var enrolledCount = await _db.Registrations.CountAsync(r => r.EventId == eventId && r.Status == RegistrationStatus.Enrolled);
            if (enrolledCount < ev.Capacity)
            {
                registration.Status = RegistrationStatus.Enrolled;
                registration.Position = null;
            }
            else
            {
                var maxPos = await _db.Registrations.Where(r => r.EventId == eventId && r.Position.HasValue).MaxAsync(r => (int?)r.Position) ?? 0;
                registration.Status = RegistrationStatus.Waitlisted;
                registration.Position = maxPos + 1;
            }

            await _db.Registrations.AddAsync(registration);
            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            return registration.Status;
        }
    }
}
