using Microsoft.EntityFrameworkCore;

namespace BookStore.Database;

public class BookStoreDbContext(DbContextOptions options) : DbContext(options)
{
	public DbSet<Book> Books { get; set; }
}
