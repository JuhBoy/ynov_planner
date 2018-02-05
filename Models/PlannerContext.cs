using Microsoft.EntityFrameworkCore;

namespace events_planner.Models {
    public class PlannerContext : DbContext {
        public DbSet<User> Users { get; set; }

        public PlannerContext(DbContextOptions options) : base(options) {}        

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            // Here the setup
        }
    }
}