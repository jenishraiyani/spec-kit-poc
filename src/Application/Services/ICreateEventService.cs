using System;
using System.Threading.Tasks;

namespace Application.Services
{
    public interface ICreateEventService
    {
        Task<Guid> CreateEventAsync(string title, string? description, string? location,
            DateTimeOffset startTime, DateTimeOffset endTime, int capacity,
            DateTimeOffset? registrationOpen, DateTimeOffset? registrationClose, string timezone);
    }
}
