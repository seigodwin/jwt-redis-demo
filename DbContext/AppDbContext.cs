using System.ComponentModel;
using JwtDemo.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JwtDemo.DbContext
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Product> Products { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
            });
            
            builder.Entity<Product>(entity =>
            {
                entity.HasIndex(p => p.Name);
                entity.HasData( new Product
                {
                        Id = 1,
                        Name = "Product 1",
                        Description = "Description for product 1",
                        Price = 10
                });
            });  
        }
    }
}