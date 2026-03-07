using System;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IAuditRepository
    {
        Task AddAsync(Audit audit);
    }
}
