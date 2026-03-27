using Microsoft.EntityFrameworkCore;
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
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<Opportunity> Opportunities => Set<Opportunity>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Contract> Contracts => Set<Contract>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Company>()
                .HasMany(c => c.Contacts)
                .WithOne(c => c.Company)
                .HasForeignKey(c => c.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Company>()
                .HasMany(c => c.Projects)
                .WithOne(p => p.Company)
                .HasForeignKey(p => p.CompanyId)
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

            modelBuilder.Entity<Project>()
                .HasMany(p => p.Contracts)
                .WithOne(ct => ct.Project)
                .HasForeignKey(ct => ct.ProjectId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<User>()
                .HasMany(u => u.CompaniesManaged)
                .WithOne(c => c.AgentResponsable)
                .HasForeignKey(c => c.AgentResponsableId)
                .OnDelete(DeleteBehavior.NoAction); // ← modifié

            modelBuilder.Entity<User>()
                .HasMany(u => u.OpportunitiesManaged)
                .WithOne(o => o.AgentCommercial)
                .HasForeignKey(o => o.AgentCommercialId)
                .OnDelete(DeleteBehavior.NoAction); // ← modifié

            modelBuilder.Entity<Project>()
                .HasMany(p => p.Opportunities)
                .WithOne(o => o.ProjectParent)
                .HasForeignKey(o => o.ProjectParentId)
                .OnDelete(DeleteBehavior.NoAction); // ← modifié

            modelBuilder.Entity<Opportunity>()
                .Property(o => o.ValeurEstimee)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Contract>()
                .Property(ct => ct.Amount)
                .HasPrecision(18, 2);
        }
    }
}
