using Microsoft.EntityFrameworkCore;
using NoorSardPlatform.Models;

namespace NoorSardPlatform.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Participant> Participants => Set<Participant>();
    }
}