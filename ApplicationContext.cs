using Microsoft.EntityFrameworkCore;
using WebApplicationJWT_Auth;

namespace WebApplicationJWT_Auth
{
    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();  
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
                    new User { Id = 1, Email = "tom@mail.com",  Password = "12345" },
                    new User { Id = 2, Email = "bob@mail.com",  Password = "55555" },
                    new User { Id = 3, Email = "daulet@mail.com", Password = "12345" }

            );
        }
    }
}
