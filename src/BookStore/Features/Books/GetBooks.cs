using BookStore.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Features.Books;

[ApiController]
public class GetBooksHandler(BookStoreDbContext dbContext) : ControllerBase
{

	private readonly BookStoreDbContext _dbContext = dbContext;

	[HttpGet]
	[Route("/books")]
	public async Task<IEnumerable<BookResponse>> GetBooks(string? author, int? year, string? publisher)
	{
		// in the case of query parameters, empty parameter values bind as null
		// thus we cannot distinguish between null and an empty string

		var query = _dbContext.Books.AsQueryable();

		if (author is not null)
			query = query.Where(m => m.Author == author);

		if (year is { } yearnum)
			query = query.Where(m => m.Year == yearnum);

		if (publisher is not null)
			query = query.Where(m => m.Publisher == publisher);

		var books = await query.Select(m => new BookResponse()
		{
			Id = m.Id,
			Title = m.Title,
			Author = m.Author,
			Year = m.Year,
			Publisher = m.Publisher,
			Description = m.Description,
		}).ToListAsync();

		return books;
	}

	[HttpGet("/books/{id}")]
	public async Task<IActionResult> GetBook(int id)
	{
		var book = await _dbContext.Books.Where(m => m.Id == id).Select(m => new BookResponse()
		{
			Id = m.Id,
			Title = m.Title,
			Author = m.Author,
			Year = m.Year,
			Publisher = m.Publisher,
			Description = m.Description,
		}).FirstOrDefaultAsync();

		return book is null ? NotFound() : Ok(book);
	}

	public class BookResponse
	{
		public required int Id { get; set; }
		public required string Title { get; set; }
		public required string Author { get; set; }
		public required int Year { get; set; }
		public required string? Publisher { get; set; }
		public required string? Description { get; set; }
	}
}
