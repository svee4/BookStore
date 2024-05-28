using BookStore.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Features.Books;

[ApiController]
public class DeleteBookHandler(BookStoreDbContext dbContext) : ControllerBase
{

	private readonly BookStoreDbContext _dbContext = dbContext;

	[HttpDelete("/books/{id:int}")]
	public async Task<IActionResult> DeleteBook(int id)
	{
		var affectedCount = await _dbContext.Books.Where(m => m.Id == id).ExecuteDeleteAsync();
		return affectedCount == 0
			? NotFound()
			: NoContent();
	}
}
