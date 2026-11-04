using Microsoft.EntityFrameworkCore;
using Consegne.Models;

namespace Consegne.Data;

public class ConsegneDbContext(DbContextOptions<ConsegneDbContext> options) : DbContext(options)
{
    public DbSet<Abstract> Abstracts => Set<Abstract>();
    public DbSet<Progetto> Progetti => Set<Progetto>();
    public DbSet<GruppoRef> Gruppi => Set<GruppoRef>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Abstract>(e =>
        {
            e.ToTable("abstracts");
            e.HasKey(x => x.IdAbstract);
            e.Property(x => x.IdAbstract).HasColumnName("id_abstract");
            e.Property(x => x.IdGruppo).HasColumnName("id_gruppo");
            e.Property(x => x.DocenteEmail).HasColumnName("docente_email");
            e.Property(x => x.Descrizione).HasColumnName("descrizione");
            e.Property(x => x.DataOraConsegna).HasColumnName("data_ora_consegna");
            e.Property(x => x.ModificheProposte).HasColumnName("modifiche_proposte");
            e.Property(x => x.Esito).HasColumnName("esito");
        });
        b.Entity<Progetto>(e =>
        {
            e.ToTable("progetti");
            e.HasKey(x => x.IdProgetto);
            e.Property(x => x.IdProgetto).HasColumnName("id_progetto");
            e.Property(x => x.IdGruppo).HasColumnName("id_gruppo");
            e.Property(x => x.DocenteEmail).HasColumnName("docente_email");
            e.Property(x => x.FileZip).HasColumnName("file_zip");
            e.Property(x => x.DataOraConsegna).HasColumnName("data_ora_consegna");
            e.Property(x => x.Valutazione).HasColumnName("valutazione");
            e.Property(x => x.Accettazione).HasColumnName("accettazione");
        });
        b.Entity<GruppoRef>(e =>
        {
            e.ToTable("gruppi");
            e.HasKey(x => x.IdGruppo);
            e.Property(x => x.IdGruppo).HasColumnName("id_gruppo");
            e.Property(x => x.IdSessione).HasColumnName("id_sessione");
            e.Property(x => x.Stato).HasColumnName("stato").HasConversion<string>();
            e.Property(x => x.CapogruppoEmail).HasColumnName("capogruppo_email");
        });
    }
}
