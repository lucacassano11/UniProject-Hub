using Microsoft.EntityFrameworkCore;
using Sicurezza.Models;

namespace Sicurezza.Data;

public class SicurezzaDbContext(DbContextOptions<SicurezzaDbContext> options) : DbContext(options)
{
    public DbSet<UtenteRef> Utenti => Set<UtenteRef>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<UtenteRef>(e =>
        {
            e.ToTable("utenti");
            e.HasKey(x => x.Email);
            e.Property(x => x.Email).HasColumnName("email");
            e.Property(x => x.Ruolo).HasColumnName("ruolo");
            e.Property(x => x.FailedAttempts).HasColumnName("failed_attempts");
            e.Property(x => x.LockedUntil).HasColumnName("locked_until");
        });
    }
}
