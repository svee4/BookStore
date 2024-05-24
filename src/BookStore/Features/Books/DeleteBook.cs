using BookStore.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Features.Books;

[ApiController]
public class DeleteBookHandler(BookStoreDbContext dbContext) : ControllerBase
{

	private readonly BookStoreDbContext _dbContext = dbContext;

	[HttpDelete("/books/{id}")]
	public async Task<IActionResult> DeleteBook(int id)
	{
		var exists = await _dbContext.Books.AnyAsync(m => m.Id == id);
		if (!exists)
		{
			return NotFound();
		}

		_ = await _dbContext.Books.Where(m => m.Id == id).ExecuteDeleteAsync();
		return NoContent();
	}
}
