using Microsoft.EntityFrameworkCore;

namespace AutoServiss.Database
{
    public class AutoServissDbContext : DbContext
    {
        public AutoServissDbContext(DbContextOptions<AutoServissDbContext> options) : base(options)
        {
        }

        public DbSet<Darbinieks> Darbinieki { get; set; }
        public DbSet<Marka> Markas { get; set; }
        public DbSet<Modelis> Modeli { get; set; }
        public DbSet<Klients> Klienti { get; set; }
        public DbSet<Adrese> Adreses { get; set; }
        public DbSet<Banka> Bankas { get; set; }    
        public DbSet<Transportlidzeklis> Transportlidzekli { get; set; }
        public DbSet<ServisaLapa> ServisaLapas { get; set; }
        public DbSet<Defekts> Defekti { get; set; }
        public DbSet<RezervesDala> RezervesDalas { get; set; }
        public DbSet<PaveiktaisDarbs> PaveiktieDarbi { get; set; }
        public DbSet<Mehanikis> Mehaniki { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Darbinieks>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Avatar).IsRequired();
                entity.Property(e => e.PilnsVards).IsRequired();
                entity.Property(e => e.Amats).IsRequired();
                entity.Property(e => e.Administrators).IsRequired();
                entity.Property(e => e.Aktivs).IsRequired();
            });

            modelBuilder.Entity<Marka>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nosaukums).IsRequired();
            });

            modelBuilder.Entity<Modelis>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nosaukums).IsRequired();
                entity.HasOne(e => e.Marka)
                    .WithMany(e => e.Modeli)
                    .HasForeignKey(e => e.MarkasId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Klients>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Veids).IsRequired();
                entity.Property(e => e.Nosaukums).IsRequired();
            });

            modelBuilder.Entity<Adrese>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Veids).IsRequired();
                entity.Property(e => e.Nosaukums).IsRequired();
                entity.HasOne(e => e.Klients)
                    .WithMany(e => e.Adreses)
                    .HasForeignKey(e => e.KlientaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Banka>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nosaukums).IsRequired();
                entity.HasOne(e => e.Klients)
                    .WithMany(e => e.Bankas)
                    .HasForeignKey(e => e.KlientaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Transportlidzeklis>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Numurs).IsRequired();
                entity.Property(e => e.Marka).IsRequired();
                entity.Property(e => e.Modelis).IsRequired();
                entity.HasOne(e => e.Klients)
                    .WithMany(e => e.Transportlidzekli)
                    .HasForeignKey(e => e.KlientaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ServisaLapa>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Datums).IsRequired();
                entity.Property(e => e.TransportlidzeklaId).IsRequired();
                entity.Property(e => e.TransportlidzeklaNumurs).IsRequired();
                entity.Property(e => e.TransportlidzeklaMarka).IsRequired();
                entity.Property(e => e.TransportlidzeklaModelis).IsRequired();
                entity.Property(e => e.TransportlidzeklaGads).IsRequired();
                entity.Property(e => e.KlientaId).IsRequired();
                entity.Property(e => e.KlientaVeids).IsRequired();
                entity.Property(e => e.KlientaNosaukums).IsRequired();
                entity.Property(e => e.KopejaSumma).IsRequired();
            });

            modelBuilder.Entity<Defekts>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Veids).IsRequired();
                entity.Property(e => e.Nosaukums).IsRequired();
                entity.HasOne(e => e.ServisaLapa)
                    .WithMany(e => e.Defekti)
                    .HasForeignKey(e => e.ServisaLapasId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<RezervesDala>(entity =>
            {
                entity.HasKey(e => e.Id);                
                entity.Property(e => e.Nosaukums).IsRequired();
                entity.Property(e => e.Skaits).IsRequired();
                entity.Property(e => e.Mervieniba).IsRequired();
                entity.Property(e => e.Cena).IsRequired();
                entity.HasOne(e => e.ServisaLapa)
                    .WithMany(e => e.RezervesDalas)
                    .HasForeignKey(e => e.ServisaLapasId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PaveiktaisDarbs>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nosaukums).IsRequired();
                entity.Property(e => e.Skaits).IsRequired();
                entity.Property(e => e.Mervieniba).IsRequired();
                entity.Property(e => e.Cena).IsRequired();
                entity.HasOne(e => e.ServisaLapa)
                    .WithMany(e => e.PaveiktieDarbi)
                    .HasForeignKey(e => e.ServisaLapasId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Mehanikis>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nosaukums).IsRequired();
                entity.HasOne(e => e.ServisaLapa)
                    .WithMany(e => e.Mehaniki)
                    .HasForeignKey(e => e.ServisaLapasId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
