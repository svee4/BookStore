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

	public sealed record Command
	{
		public required int Id { get; init; }
	}

	internal static Results<NoContent, NotFound> TransformResult(bool result) =>
		result ? TypedResults.NoContent() : TypedResults.NotFound();
	
	private static async ValueTask<bool> HandleAsync(
		Command command,
		BookStoreDbContext dbContext,
		CancellationToken cancellationToken
	) =>
		await dbContext.Books
			.Where(m => m.Id == command.Id)
			.ExecuteDeleteAsync(cancellationToken)
		> 0;
}

