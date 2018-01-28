using Microsoft.EntityFrameworkCore;

namespace events_planner.Models {
    public class PlannerContext : DbContext {
        public PlannerContext(DbContextOptions<PlannerContext> options) : base(options) { /*...*/ }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            // Here the setup
        }
    }
}