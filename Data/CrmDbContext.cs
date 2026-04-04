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
        public DbSet<Opportunity> Opportunities => Set<Opportunity>();
        public DbSet<Contract> Contracts => Set<Contract>();
        public DbSet<Phase> Phases => Set<Phase>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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

            modelBuilder.Entity<Opportunity>()
                .Property(o => o.ValeurEstimee)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Phase>()
                .Property(p => p.Montant)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Contract>()
                .Property(ct => ct.Amount)
                .HasPrecision(18, 2);
        }
    }
}
