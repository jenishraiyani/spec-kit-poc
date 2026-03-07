using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IRegistrationRepository
    {
        Task AddAsync(Registration registration);
        Task<Registration?> GetByIdAsync(Guid id);
        Task<bool> ExistsActiveRegistrationAsync(Guid eventId, Guid residentId);
        Task<List<Registration>> GetWaitlistAsync(Guid eventId, int limit = 50);
        Task UpdateAsync(Registration registration);
        Task<RegistrationStatus> EnrollOrWaitlistAsync(Guid eventId, Registration registration);
    }
}
