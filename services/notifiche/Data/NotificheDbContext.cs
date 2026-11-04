using Microsoft.EntityFrameworkCore;
using Notifiche.Models;

namespace Notifiche.Data;

public class NotificheDbContext(DbContextOptions<NotificheDbContext> options) : DbContext(options)
{
    public DbSet<Notifica> Notifiche => Set<Notifica>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Notifica>(e =>
        {
            e.ToTable("notifiche");
            e.HasKey(x => x.IdNotifica);
            e.Property(x => x.IdNotifica).HasColumnName("id_notifica");
            e.Property(x => x.DestinatarioEmail).HasColumnName("destinatario_email");
            e.Property(x => x.Tipo).HasColumnName("tipo");
            e.Property(x => x.Messaggio).HasColumnName("messaggio");
            e.Property(x => x.Letta).HasColumnName("letta");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
        });
    }
}
