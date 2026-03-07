using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Event> Events { get; set; } = null!;
        public DbSet<Resident> Residents { get; set; } = null!;
        public DbSet<Registration> Registrations { get; set; } = null!;
        public DbSet<Domain.Entities.Audit> Audits { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Event>(eb =>
            {
                eb.HasKey(e => e.Id);
                eb.Property(e => e.Title).HasMaxLength(200).IsRequired();
                eb.Property(e => e.Timezone).HasMaxLength(64).IsRequired();
            });

            modelBuilder.Entity<Resident>(rb =>
            {
                rb.HasKey(r => r.Id);
                rb.Property(r => r.Email).HasMaxLength(256).IsRequired();
                rb.Property(r => r.Name).HasMaxLength(200).IsRequired();
            });

            modelBuilder.Entity<Registration>(rb =>
            {
                rb.HasKey(r => r.Id);
                rb.Property(r => r.Status).IsRequired();
                rb.Property(r => r.CreatedAt).IsRequired();
                rb.Property(r => r.UpdatedAt).IsRequired();
                rb.HasIndex(r => new { r.EventId, r.Status, r.Position });
                rb.HasIndex(r => new { r.EventId, r.ResidentId }).IsUnique();
            });

            modelBuilder.Entity<Domain.Entities.Audit>(ab =>
            {
                ab.HasKey(a => a.Id);
                ab.Property(a => a.Action).HasMaxLength(100).IsRequired();
                ab.Property(a => a.EntityName).HasMaxLength(100).IsRequired();
                ab.Property(a => a.PerformedBy).HasMaxLength(200).IsRequired();
                ab.Property(a => a.PerformedAt).IsRequired();
            });
        }
    }
}
