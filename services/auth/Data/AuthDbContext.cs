using Auth.Models;
using Microsoft.EntityFrameworkCore;

namespace Auth.Data;

public class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
    public DbSet<Utente> Utenti => Set<Utente>();
    public DbSet<Studente> Studenti => Set<Studente>();
    public DbSet<Docente> Docenti => Set<Docente>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Utente>(e =>
        {
            e.ToTable("utenti");
            e.HasKey(x => x.Email);
            e.Property(x => x.Email).HasColumnName("email");
            e.Property(x => x.PasswordHash).HasColumnName("password_hash");
            e.Property(x => x.Nome).HasColumnName("nome");
            e.Property(x => x.Cognome).HasColumnName("cognome");
            e.Property(x => x.Ruolo).HasColumnName("ruolo").HasConversion<string>();
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.FailedAttempts).HasColumnName("failed_attempts");
            e.Property(x => x.LockedUntil).HasColumnName("locked_until");
        });
        b.Entity<Studente>(e =>
        {
            e.ToTable("studenti");
            e.HasKey(x => x.Email);
            e.Property(x => x.Email).HasColumnName("email");
            e.Property(x => x.Matricola).HasColumnName("matricola");
            e.HasOne<Utente>().WithMany().HasForeignKey(x => x.Email);
        });
        b.Entity<Docente>(e =>
        {
            e.ToTable("docenti");
            e.HasKey(x => x.Email);
            e.Property(x => x.Email).HasColumnName("email");
            e.HasOne<Utente>().WithMany().HasForeignKey(x => x.Email);
        });
    }
}
