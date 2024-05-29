using BookStore.Database;
using Immediate.Apis.Shared;
using Immediate.Handlers.Shared;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Features.Books.Endpoints;

[Handler]
[MapGet("/books/{id:int}")]
public static partial class GetBook
{

	public sealed record Query
	{
		public required int Id { get; init; }
	}
	
	internal static Results<Ok<Book>, NotFound> TransformResult(Book? result) =>
		result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();

	private static async ValueTask<Book?> HandleAsync(
		Query query, 
		BookStoreDbContext dbContext,
		CancellationToken cancellationToken
	) => 
		await dbContext.Books
			.Where(m => m.Id == query.Id)
			.Select(m => new Book(m.Id, m.Title, m.Author, m.Year, m.Publisher, m.Description))
			.FirstOrDefaultAsync(cancellationToken);

	public record Book(
		int Id,
		string Title,
		string Author,
		int Year,
		string? Publisher,
		string? Description);
}
