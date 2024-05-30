using BookStore.Database;
using Immediate.Apis.Shared;
using Immediate.Handlers.Shared;
using Immediate.Validations.Shared;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Features.Books.Endpoints;

[Handler]
[MapPost("/books")]
public static partial class AddBook
{
	
	// ReSharper disable once ClassNeverInstantiated.Global
	[Validate]
	public sealed partial record Command : IValidationTarget<Command>
	{
		[NotEmpty]
		public required string Title { get; init; } 
		
		[NotEmpty]
		public required string Author { get; init; } 
		public required int Year { get; init; }
		
		[NotEmpty]
		public string? Publisher { get; init; }
		public string? Description { get; init; }
	}

	internal static Results<Ok<IdResponse>, BadRequest<ValidationProblemDetails>> TransformResult(Response result) =>
		result switch
		{
			IdResponse d => TypedResults.Ok(d),
			DuplicateResponse d => TypedResults.BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>()
			{
				["Model"] = [d.ErrorMessage]
			})
			{
				Detail = d.ErrorMessage
			}),
			_ => throw new InvalidOperationException("Unhandled response type")
		};

	private static async ValueTask<Response> HandleAsync(
		Command command,
		BookStoreDbContext dbContext,
		CancellationToken cancellationToken
	)
	{
		var title = command.Title.Trim();
		var author = command.Author.Trim();
		var year = command.Year; // for consistency
		var publisher = command.Publisher?.Trim();
		var description = command.Description?.Trim();
		
		if (dbContext.Books.Any(m =>
			    m.Title == title
			    && m.Author == author
			    && m.Year == year))
		{
			return new DuplicateResponse();
		}
		
		var book = Book.CreateNew(
			title,
			author,
			year,
			publisher,
			description
		);
		
		await dbContext.Books.AddAsync(book, cancellationToken);
		await dbContext.SaveChangesAsync(cancellationToken);
		return new IdResponse(book.Id);
	}

	public abstract record Response;
	public sealed record IdResponse(int Id) : Response;
	public sealed record DuplicateResponse : Response
	{
		public string ErrorMessage => "A book with the same title, author and year already exists";
	}
}
