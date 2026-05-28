using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using ModuleCRM.Models;

namespace ModuleCRM.Data
{
    public class CrmDbContext : DbContext
    {
        public CrmDbContext(DbContextOptions<CrmDbContext> options)
            : base(options)
        {
        }

        public DbSet<Company> Companies => Set<Company>();
        public DbSet<Contact> Contacts => Set<Contact>();
        public DbSet<Opportunity> Opportunities => Set<Opportunity>();
        public DbSet<Contract> Contracts => Set<Contract>();
        public DbSet<Phase> Phases => Set<Phase>();
        public DbSet<Meeting> Meetings => Set<Meeting>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<Projet> Projets => Set<Projet>();
        public DbSet<ProjetPhase> ProjetPhases => Set<ProjetPhase>();
        public DbSet<ProjetTache> ProjetTaches => Set<ProjetTache>();
        public DbSet<ProjetSousTache> ProjetSousTaches => Set<ProjetSousTache>();
        public DbSet<TypeProjet> TypesProjet => Set<TypeProjet>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<UserSetting> UserSettings => Set<UserSetting>();
        public DbSet<AgentLocal> AgentsLocal => Set<AgentLocal>();
        public DbSet<EquipeLocal> EquipesLocal => Set<EquipeLocal>();
        public DbSet<EquipeMembreLocal> EquipesMembresLocal => Set<EquipeMembreLocal>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Relationships ---
            modelBuilder.Entity<Company>()
                .HasMany(c => c.Contacts)
                .WithOne(c => c.Company)
                .HasForeignKey(c => c.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Company>()
                .HasMany(c => c.Opportunities)
                .WithOne(o => o.Company)
                .HasForeignKey(o => o.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Company>()
                .HasMany(c => c.Contracts)
                .WithOne(ct => ct.Company)
                .HasForeignKey(ct => ct.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Opportunity>()
                .HasMany(o => o.Phases)
                .WithOne(p => p.Opportunity)
                .HasForeignKey(p => p.OpportunityId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Phase>()
                .HasMany(p => p.Meetings)
                .WithOne(m => m.Phase)
                .HasForeignKey(m => m.PhaseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Phase>()
                .Property(p => p.Montant)
                .HasPrecision(18, 2);

            // --- Precision ---
            modelBuilder.Entity<Opportunity>()
                .Property(o => o.ValeurEstimee)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Phase>()
                .Property(p => p.Montant)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Contract>()
                .Property(ct => ct.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Projet>()
                .Property(p => p.BudgetEstime)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Projet>()
                .Property(p => p.BudgetReel)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Projet>()
                .HasIndex(p => p.OpportuniteIdOrigine);

            // --- Projet hierarchy (Phase > Tache > SousTache) ---
            modelBuilder.Entity<Projet>()
                .HasMany(p => p.Phases)
                .WithOne(ph => ph.Projet)
                .HasForeignKey(ph => ph.ProjetId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProjetPhase>()
                .HasMany(ph => ph.Taches)
                .WithOne(t => t.Phase)
                .HasForeignKey(t => t.ProjetPhaseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProjetTache>()
                .HasMany(t => t.SousTaches)
                .WithOne(s => s.Tache)
                .HasForeignKey(s => s.ProjetTacheId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProjetSousTache>()
                .Property(s => s.DureeEstimeeHeures)
                .HasPrecision(10, 2);

            // --- UNIQUE constraints ---
            modelBuilder.Entity<Contact>()
                .HasIndex(c => c.Login)
                .IsUnique()
                .HasFilter("[Login] IS NOT NULL AND [Login] <> ''");

            modelBuilder.Entity<Contact>()
                .HasIndex(c => c.Email)
                .IsUnique();

            modelBuilder.Entity<Contact>()
                .HasIndex(c => c.CompanyId);

            modelBuilder.Entity<Projet>()
                .HasOne(p => p.Company)
                .WithMany()
                .HasForeignKey(p => p.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Soft delete query filters ---
            modelBuilder.Entity<Company>()
                .HasQueryFilter(c => !c.IsDeleted);

            modelBuilder.Entity<Contact>()
                .HasQueryFilter(c => !c.IsDeleted);

            modelBuilder.Entity<Opportunity>()
                .HasQueryFilter(o => !o.IsDeleted);

            modelBuilder.Entity<Contract>()
                .HasQueryFilter(ct => !ct.IsDeleted);

            // --- TypeProjet ---
            modelBuilder.Entity<TypeProjet>()
                .HasIndex(t => t.Value)
                .IsUnique();

            modelBuilder.Entity<TypeProjet>()
                .HasIndex(t => t.TypeProjetGuid)
                .IsUnique();

            // --- AuditLog indexes ---
            modelBuilder.Entity<AuditLog>()
                .HasIndex(a => a.EntityName);

            modelBuilder.Entity<AuditLog>()
                .HasIndex(a => a.Timestamp);

            // --- Notifications ---
            modelBuilder.Entity<Notification>()
                .HasIndex(n => new { n.UserId, n.IsRead });

            modelBuilder.Entity<Notification>()
                .HasIndex(n => n.CreatedAt);

            // --- UserSettings ---
            modelBuilder.Entity<UserSetting>()
                .HasIndex(s => s.UserId)
                .IsUnique();

            // --- AgentLocal (read-replica RH via RabbitMQ) ---
            // Clé composite Id + AgentType car les ids interne/externe peuvent collisionner.
            modelBuilder.Entity<AgentLocal>()
                .HasKey(a => new { a.Id, a.AgentType });

            modelBuilder.Entity<AgentLocal>()
                .Property(a => a.Id)
                .ValueGeneratedNever();

            modelBuilder.Entity<AgentLocal>()
                .Property(a => a.CoutHoraire)
                .HasPrecision(18, 2);

            modelBuilder.Entity<AgentLocal>()
                .Property(a => a.Rating)
                .HasPrecision(5, 2);

            modelBuilder.Entity<AgentLocal>()
                .HasIndex(a => a.Email);

            modelBuilder.Entity<AgentLocal>()
                .HasIndex(a => a.Role);

            modelBuilder.Entity<AgentLocal>()
                .HasIndex(a => a.Departement);

            // --- EquipeLocal (read-replica via RabbitMQ) ---
            modelBuilder.Entity<EquipeLocal>()
                .HasIndex(e => e.EquipeGuid)
                .IsUnique();

            modelBuilder.Entity<EquipeLocal>()
                .HasMany(e => e.Membres)
                .WithOne(m => m.Equipe)
                .HasForeignKey(m => m.EquipeLocalId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
