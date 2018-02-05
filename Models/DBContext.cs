using Microsoft.EntityFrameworkCore;

namespace events_planner.Models {
    public class PlannerContext : DbContext {
        public DbSet<Booking> Booking { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Event> Event { get; set; }
        public DbSet<Price> Price { get; set; }
        public DbSet<Promotion> Promotion { get; set; }
        public DbSet<Recovery> Recovery { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<Subscribe> Subscribe { get; set; }
        public DbSet<User> User { get; set; }
        
        

        public PlannerContext(DbContextOptions options) : base(options) {}        

        protected override void OnModelCreating(ModelBuilder modelBuilder) {

        }
    }
}