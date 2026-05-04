using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<IssuedBook> IssuedBooks { get; set; }
    }
}