using Microsoft.EntityFrameworkCore;
using PetClaims.Api.Entities;

namespace PetClaims.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Policy> Policies { get; set; }

        public DbSet<Claim> Claims { get; set; }

        public DbSet<ClaimItem> ClaimItems { get; set; }

        public DbSet<ClaimDecision> ClaimDecisions { get; set; }

        public DbSet<ClaimDocument> ClaimDocuments { get; set; }
    }
}