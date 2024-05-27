using System.ComponentModel.DataAnnotations;
using BookStore.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Features.Books;

[ApiController]
public class AddBookHandler(BookStoreDbContext dbContext) : ControllerBase
{

	private readonly BookStoreDbContext _dbContext = dbContext;

	[HttpPost("/books")]
	public async Task<IActionResult> AddBook(AddBookRequest model)
	{
		ArgumentNullException.ThrowIfNull(model);

		var exists = await _dbContext.Books.AnyAsync(m =>
			   m.Title == model.Title
			&& m.Author == model.Author
			&& m.Year == model.Year);

		if (exists)
		{
			ModelState.AddModelError("Model", "A book with the same title, author and year already exists");
			return ValidationProblem(ModelState);
		}

		var book = Book.CreateNew(
			title: model.Title,
			author: model.Author,
			year: model.Year,
			publisher: model.Publisher,
			description: model.Description);

		_ = await _dbContext.Books.AddAsync(book);
		_ = await _dbContext.SaveChangesAsync();

		return Ok(new AddBookResponse() { Id = book.Id });
	}

	public class AddBookRequest : IValidatableObject
	{
		// required attribute is needed for the non-empty string check
		[Required]
		public required string Title { get; set; } = default!;
		[Required]
		public required string Author { get; set; } = default!;
		public required int Year { get; set; }
		public string? Publisher { get; set; }
		public string? Description { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			Title = Title.Trim();
			Author = Author.Trim();
			
			// since Publisher can be null, we need to do this validation manually
			if (Publisher is not null)
			{
				if (string.IsNullOrWhiteSpace(Publisher))
				{
					yield return new ValidationResult($"The field {nameof(Publisher)} may not be empty (but it may be null)", [nameof(Publisher)]);
				}
				else
				{
					Publisher = Publisher.Trim();
				}
			}

			if (Description is not null)
			{
				Description = Description.Trim();
			}
		}
	}

	public class AddBookResponse
	{
		public required int Id { get; set; }
	}
}
