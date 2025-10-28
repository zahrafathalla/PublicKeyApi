using Microsoft.EntityFrameworkCore;
using PublicKeyApi.Entities;

namespace PublicKeyApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<IntegrationClient> IntegrationClients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<IntegrationClient>()
                .HasIndex(ic => ic.ClientIdentifier)
                .IsUnique();
        }
    }
}
