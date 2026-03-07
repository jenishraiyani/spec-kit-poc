using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class AuditRepository : IAuditRepository
    {
        private readonly AppDbContext _db;
        public AuditRepository(AppDbContext db) => _db = db;

        public async Task AddAsync(Audit audit)
        {
            await _db.Set<Audit>().AddAsync(audit);
            await _db.SaveChangesAsync();
        }
    }
}
