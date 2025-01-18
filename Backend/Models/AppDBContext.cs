using Microsoft.EntityFrameworkCore;

namespace Backend.Models
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {

        }

        public AppDBContext()
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

        public class Seeder
        {
            public static void SeedDatabase(AppDBContext context)
            {
                if (!context.CategoryProducts.Any())
                {
                    var categories = new List<CategoryProduct>();
                    for (int i = 1; i <= 20; i++)
                    {
                        categories.Add(new CategoryProduct
                        {
                            Name = $"Loại sản phẩm {i}",
                            CreatedDate = DateTime.Now.AddDays(-i)
                        });
                    }

                    context.CategoryProducts.AddRange(categories);
                    context.SaveChanges();

                    var categoryList = context.CategoryProducts.ToList();

                    var products = new List<Product>();
                    var random = new Random();
                    for (int i = 1; i <= 10000; i++)
                    {
                        var randomCategory = categoryList[random.Next(categoryList.Count)];
                        products.Add(new Product
                        {
                            Name = $"Sản phẩm {i}",
                            Price = (float)(random.NextDouble() * 100),
                            CategoryProductId = randomCategory.Id,
                            CreatedDate = DateTime.Now.AddDays(-random.Next(1, 100))
                        });
                    }

                    context.Products.AddRange(products);
                    context.SaveChanges();
                }
            }
        }
    }
}
