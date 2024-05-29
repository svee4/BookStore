using BookStore.Database;
using Immediate.Apis.Shared;
using Immediate.Handlers.Shared;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Features.Books.Endpoints;

[Handler]
[MapDelete("/books/{id:int}")]
public static partial class DeleteBook
{

	public sealed record Query
	{
		public required int Id { get; init; }
	}

	internal static Results<NoContent, NotFound> TransformResult(bool result) =>
		result ? TypedResults.NoContent() : TypedResults.NotFound();
	
	private static async ValueTask<bool> HandleAsync(
		Query query,
		BookStoreDbContext dbContext,
		CancellationToken cancellationToken
	) =>
		await dbContext.Books
			.Where(m => m.Id == query.Id)
			.ExecuteDeleteAsync(cancellationToken)
		> 0;
}

