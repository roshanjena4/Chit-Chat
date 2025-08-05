using Microsoft.EntityFrameworkCore;

namespace ChatApplication.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Message>().ToTable("Message"); // Case-sensitive match
        }


        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }
    }
}
