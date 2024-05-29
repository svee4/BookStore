using IntegrationTests.Helpers;
using Xunit.Abstractions;

namespace IntegrationTests;

public partial class BasicTests
{

    static class GetBooksModels
    {
        public record Success(
            int Id,
            string Title,
            string Author,
            int Year,
            string? Publisher,
            string? Description);
    }
    
    [Fact]
    public async Task GetBooks_IsEmpty()
    {
        var client = _webApplicationFactory.CreateClient();
        await DatabaseHelpers.EnsureMigrations(_webApplicationFactory.Services);
        await DatabaseHelpers.ClearBooks(_webApplicationFactory.Services);

        var response = await client.GetFromJsonAsync<IEnumerable<GetBooksModels.Success>>("/books");
        
        Assert.NotNull(response);
        Assert.Empty(response);
    }
    
    [Fact]
    public async Task GetBooks_Search()
    {
        var client = _webApplicationFactory.CreateClient();
        await DatabaseHelpers.EnsureMigrations(_webApplicationFactory.Services);
        await DatabaseHelpers.ClearBooks(_webApplicationFactory.Services);

        var book1 = new { Title = "X1", Author = "A1", Year = 2, Publisher = "P1" };
        var book1Model = new GetBooksModels.Success(
            await AddBook(client, book1),
            book1.Title,
            book1.Author,
            book1.Year,
            book1.Publisher,
            null
        );
        
        var book2 = new { Title = "X2", Author = "A2", Year = 2, Publisher = "P2" };
        var book2Model = new GetBooksModels.Success(
            await AddBook(client, book2),
            book2.Title,
            book2.Author,
            book2.Year,
            book2.Publisher,
            null
        );
        
        var book3 = new { Title = "X3", Author = "A3", Year = 3, Publisher = "P1"};
        var book3Model = new GetBooksModels.Success(
            await AddBook(client, book3),
            book3.Title,
            book3.Author,
            book3.Year,
            book3.Publisher,
            null
        );

        {
            _testOutputHelper.WriteLine("get all books");
            var response = await client.GetFromJsonAsync<IList<GetBooksModels.Success>>("/books");
            Assert.Equal([book1Model, book2Model, book3Model], response);
        }
        
        {
            _testOutputHelper.WriteLine("get books with author A1");
            var response = await client.GetFromJsonAsync<IList<GetBooksModels.Success>>("/books?author=A1");
            Assert.Equal([book1Model], response);
        }
        
        {
            _testOutputHelper.WriteLine("get books with year 2");
            var response = await client.GetFromJsonAsync<IList<GetBooksModels.Success>>("/books?year=2");
            Assert.Equal([book1Model, book2Model], response);
        }
        
        {
            _testOutputHelper.WriteLine("get books with publisher P1");
            var response = await client.GetFromJsonAsync<IList<GetBooksModels.Success>>("/books?publisher=P1");
            Assert.Equal([book1Model, book3Model], response);
        }
        
        {
            _testOutputHelper.WriteLine("get one book");
            var response = await client.GetFromJsonAsync<IList<GetBooksModels.Success>>("/books?author=A1&year=2&publisher=P1");
            Assert.Equal([book1Model], response);
        }

        
        {
            _testOutputHelper.WriteLine("non existent");
            var response = await client.GetFromJsonAsync<IList<GetBooksModels.Success>>("/books?author=Z");
            Assert.Equal([], response);
        }
        
        
        static async Task<int> AddBook<TBook>(HttpClient client, TBook book)
        {
            var response = await client.PostAsJsonAsync("/books", book);
            var data = await response.Content.ReadFromJsonAsync<AddBookModels.Success>();
            response.EnsureSuccessStatusCode();
            Assert.NotNull(data);
            Assert.NotEqual(0, data.Id);
            return data.Id;
        }
    }
}