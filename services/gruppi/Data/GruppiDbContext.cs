using Microsoft.EntityFrameworkCore;
using Gruppi.Models;

namespace Gruppi.Data;

public class GruppiDbContext(DbContextOptions<GruppiDbContext> options) : DbContext(options)
{
    public DbSet<Gruppo> Gruppi => Set<Gruppo>();
    public DbSet<MembroGruppo> Membri => Set<MembroGruppo>();
    public DbSet<Richiesta> Richieste => Set<Richiesta>();
    public DbSet<Invito> Inviti => Set<Invito>();
    public DbSet<RichiestaAccesso> RichiesteAccesso => Set<RichiestaAccesso>();
    public DbSet<RichiestaDimensione> RichiesteDimensione => Set<RichiestaDimensione>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Gruppo>(e =>
        {
            e.ToTable("gruppi");
            e.HasKey(x => x.IdGruppo);
            e.Property(x => x.IdGruppo).HasColumnName("id_gruppo");
            e.Property(x => x.IdSessione).HasColumnName("id_sessione");
            e.Property(x => x.Dimensione).HasColumnName("dimensione");
            e.Property(x => x.Stato).HasColumnName("stato").HasConversion<string>();
            e.Property(x => x.CapogruppoEmail).HasColumnName("capogruppo_email");
        });
        b.Entity<MembroGruppo>(e =>
        {
            e.ToTable("membri_gruppo");
            e.HasKey(x => new { x.IdGruppo, x.StudenteEmail });
            e.Property(x => x.IdGruppo).HasColumnName("id_gruppo");
            e.Property(x => x.StudenteEmail).HasColumnName("studente_email");
            e.Property(x => x.DataIngresso).HasColumnName("data_ingresso");
        });
        b.Entity<Richiesta>(e =>
        {
            e.ToTable("richieste");
            e.HasKey(x => x.IdRichiesta);
            e.Property(x => x.IdRichiesta).HasColumnName("id_richiesta");
            e.Property(x => x.Tipo).HasColumnName("tipo").HasConversion<string>();
            e.Property(x => x.Esito).HasColumnName("esito");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
        });
        b.Entity<Invito>(e =>
        {
            e.ToTable("inviti");
            e.HasKey(x => x.IdRichiesta);
            e.Property(x => x.IdRichiesta).HasColumnName("id_richiesta").ValueGeneratedNever();
            e.Property(x => x.IdGruppo).HasColumnName("id_gruppo");
            e.Property(x => x.StudenteEmail).HasColumnName("studente_email");
        });
        b.Entity<RichiestaAccesso>(e =>
        {
            e.ToTable("richieste_accesso");
            e.HasKey(x => x.IdRichiesta);
            e.Property(x => x.IdRichiesta).HasColumnName("id_richiesta").ValueGeneratedNever();
            e.Property(x => x.IdGruppo).HasColumnName("id_gruppo");
            e.Property(x => x.StudenteEmail).HasColumnName("studente_email");
        });
        b.Entity<RichiestaDimensione>(e =>
        {
            e.ToTable("richieste_dimensione_gruppo");
            e.HasKey(x => x.IdRichiesta);
            e.Property(x => x.IdRichiesta).HasColumnName("id_richiesta").ValueGeneratedNever();
            e.Property(x => x.IdSessione).HasColumnName("id_sessione");
            e.Property(x => x.IdGruppo).HasColumnName("id_gruppo");
            e.Property(x => x.Descrizione).HasColumnName("descrizione");
        });
    }
}
