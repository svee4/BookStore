using System.Diagnostics.CodeAnalysis;

namespace BookStore.Database;

public class Book
{

	public int Id { get; set; }
	public required string Title { get; set; }
	public required string Author { get; set; }
	public required int Year { get; set; }
	public string? Publisher { get; set; }
	public string? Description { get; set; }

	private Book() { }

	public static Book CreateNew(
		string title,
		string author,
		int year,
		string? publisher = null,
		string? description = null)
	{
		ArgumentException.ThrowIfNullOrEmpty(title, nameof(title));
		ArgumentException.ThrowIfNullOrEmpty(author, nameof(author));

		if (publisher is { Length: 0 })
			ThrowForPublisher();

		return new Book()
		{
			Title = title,
			Author = author,
			Year = year,
			Publisher = publisher,
			Description = description
		};

		// a throw statement negatively affects the inlining of a method, affecting performance.
		// by extracting the throw statement into a separate method,
		// the calling method is more likely to be inlined
		[DoesNotReturn]
		static void ThrowForPublisher() =>
			throw new ArgumentException($"{nameof(publisher)} may not be empty. It may be null", nameof(publisher));
	}
}
