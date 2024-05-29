using BookStore.Database;
using Immediate.Apis.Shared;
using Immediate.Handlers.Shared;
using Immediate.Validations.Shared;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Features.Books.Endpoints;

[Handler]
[MapGet("/books/")]
public static partial class GetBooks
{
	public sealed record Query
	{
		[NotEmpty]
		public string? Author { get; init; }
		
		public int? Year { get; init; }
		
		[NotEmpty]
		public string? Publisher { get; init; }
	};

	private static async ValueTask<IEnumerable<Book>> HandleAsync(
		Query query,
		BookStoreDbContext dbContext,
		CancellationToken cancellationToken
	)
	{
		var queryBuilder = dbContext.Books.AsQueryable();

		if (query.Author is not null)
			queryBuilder = queryBuilder.Where(m => m.Author == query.Author);

		if (query.Year is { } yearnum)
			queryBuilder = queryBuilder.Where(m => m.Year == yearnum);

		if (query.Publisher is not null)
			queryBuilder = queryBuilder.Where(m => m.Publisher == query.Publisher);

		return await queryBuilder
			.Select(m => new Book(m.Id, m.Title, m.Author,  m.Year,  m.Publisher, m.Description))
			.ToListAsync(cancellationToken);
	}

	public record Book(
		int Id,
		string Title,
		string Author,
		int Year,
		string? Publisher,
		string? Description);
}
