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
        public DbSet<IntegrationClientKey> IntegrationClientKeys { get; set; }
        public DbSet<UsedNonce> UsedNonces { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<IntegrationClient>()
                .HasIndex(ic => ic.ClientIdentifier)
                .IsUnique();
        }
    }
}
