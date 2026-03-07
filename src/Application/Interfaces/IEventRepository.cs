using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IEventRepository
    {
        Task<Event?> GetByIdAsync(Guid id);
        Task AddAsync(Event @event);
        Task UpdateAsync(Event @event);
        Task<int> CountEnrolledAsync(Guid eventId);
        Task<List<Event>> GetEventsAsync(DateTimeOffset? from = null, DateTimeOffset? to = null);
    }
}
