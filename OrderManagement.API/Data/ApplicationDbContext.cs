using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OrderManagement.API.Models;
using OrderManagement.API.Models.Audit;
using OrderManagement.API.Models.Identity;


namespace OrderManagement.API.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>    //   DbContext
     
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        //Existing Tables
        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        // New Table
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        // New Table ..this is for Audit Logging
        public DbSet<AuditLog> AuditLogs { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Customer>()
                .HasIndex(customer => customer.Email)
                .IsUnique();

            modelBuilder.Entity<RefreshToken>()
               .HasOne(rt => rt.User)
                 .WithMany(u => u.RefreshTokens)  //Ye One User → Many Refresh Tokens relationship ko explicitly represent karta hai
    .HasForeignKey(rt => rt.UserId)
    .OnDelete(DeleteBehavior.Cascade);
   
            //.WithMany()  // This is basic version, but we can also specify the navigation property in ApplicationUser class
            //.HasForeignKey(rt => rt.UserId)
            //.OnDelete(DeleteBehavior.Cascade);

            // Seed Products
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Laptop", Price = 60000m, Stock = 20, Description = "High performance laptop" },
                new Product { Id = 2, Name = "Smartphone", Price = 25000m, Stock = 50, Description = "Latest smartphone" },
                new Product { Id = 3, Name = "Wireless Mouse", Price = 1500m, Stock = 100, Description = "Ergonomic wireless mouse" }
            );

            // Seed Customers
            modelBuilder.Entity<Customer>().HasData(
                new Customer { Id = 1, Name = "Pranaya Rout", Email = "pranaya@example.com" },
                new Customer { Id = 2, Name = "Sneha Das", Email = "sneha@example.com" }
            );
        }
    }
}
