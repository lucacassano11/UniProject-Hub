using Microsoft.EntityFrameworkCore;
using Log.Models;

namespace Log.Data;

public class LogDbContext(DbContextOptions<LogDbContext> options) : DbContext(options)
{
    public DbSet<Evento> Eventi => Set<Evento>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Evento>(e =>
        {
            e.ToTable("log_eventi");
            e.HasKey(x => x.IdEvento);
            e.Property(x => x.IdEvento).HasColumnName("id_evento");
            e.Property(x => x.Tipo).HasColumnName("tipo");
            e.Property(x => x.AttoreEmail).HasColumnName("attore_email");
            e.Property(x => x.Dettaglio).HasColumnName("dettaglio");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
        });
    }
}
