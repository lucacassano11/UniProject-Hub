using Microsoft.EntityFrameworkCore;
using Sessioni.Models;

namespace Sessioni.Data;

public class SessioniDbContext(DbContextOptions<SessioniDbContext> options) : DbContext(options)
{
    public DbSet<Sessione> Sessioni => Set<Sessione>();
    public DbSet<Appello> Appelli => Set<Appello>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Sessione>(e =>
        {
            e.ToTable("sessioni");
            e.HasKey(x => x.IdSessione);
            e.Property(x => x.IdSessione).HasColumnName("id_sessione");
            e.Property(x => x.VotoMassimo).HasColumnName("voto_massimo");
            e.Property(x => x.DataCreazioneSessione).HasColumnName("data_creazione_sessione");
            e.Property(x => x.InizioInvioAbstract).HasColumnName("inizio_invio_abstract");
            e.Property(x => x.DocenteEmail).HasColumnName("docente_email");
        });
        b.Entity<Appello>(e =>
        {
            e.ToTable("appelli");
            e.HasKey(x => x.IdAppello);
            e.Property(x => x.IdAppello).HasColumnName("id_appello");
            e.Property(x => x.IdSessione).HasColumnName("id_sessione");
            e.Property(x => x.DataOraAppello).HasColumnName("data_ora_appello");
            e.Property(x => x.ScadenzaConsegnaProgetto).HasColumnName("scadenza_consegna_progetto");
        });
    }
}
