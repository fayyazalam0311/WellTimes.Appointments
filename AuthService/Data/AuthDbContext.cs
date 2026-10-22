using AuthService.Models; 
using Microsoft.EntityFrameworkCore; 

namespace AuthService.Data // Define the namespace for the AuthDbContext class
{
    public class AuthDbContext : DbContext // Define the AuthDbContext class that inherits from DbContext
    {
        // Constructor that accepts DbContextOptions and passes them to the base class constructor
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }
        // Define a DbSet for the User entity
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the User entity using Fluent API
            modelBuilder.Entity<User>(entity =>
            {
                // Set the primary key for the User entity
                entity.HasKey(e=>e.Id);
                entity.HasIndex(e => e.Email).IsUnique(); // Has Index for fast lookups and uniqueness constraint on Email
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100); 
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Role).IsRequired();
                entity.Property(e => e.CreatedAtUtc).IsRequired();
            });
        }
    }
}
