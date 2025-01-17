using Microsoft.EntityFrameworkCore;

namespace Backend.Models
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CategoryProduct>(entity =>
            {
                entity.Property(c => c.Id).ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(p => p.Id).ValueGeneratedOnAdd();
            });
        }

        public DbSet<CategoryProduct> CategoryProducts { get; set; }
        public DbSet<Product> Products { get; set; }
    }
}
