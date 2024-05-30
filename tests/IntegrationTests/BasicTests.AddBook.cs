using IntegrationTests.Helpers;

namespace IntegrationTests;

public partial class BasicTests
{
    
    public static class AddBookModels
    {
        // ReSharper disable once ClassNeverInstantiated.Global
        public record Success(int Id);

        // ReSharper disable once ClassNeverInstantiated.Global
        public record Failure(
            string Title,
            int Status,
            string Detail,
            Dictionary<string, string[]> Errors
        );
    }
    
    [Fact]
    public async Task AddBook_IsSuccessful()
    {
        var client = _webApplicationFactory.CreateClient();
        await DatabaseHelpers.EnsureMigrations(_webApplicationFactory.Services);
        await DatabaseHelpers.ClearBooks(_webApplicationFactory.Services);

        // add book
        
        var request = new
        {
            title = "A",
            author = "B",
            year = 2,
            publisher = "C",
            Description = "d"
        };
        
        var response = await client.PostAsJsonAsync("/books", request);
        var data = await response.Content.ReadFromJsonAsync<AddBookModels.Success>();

        response.EnsureSuccessStatusCode();
        Assert.NotNull(data);
        Assert.NotEqual(0, data.Id);

        // ensure book data is correct
        
        var bookData = await client.GetFromJsonAsync<GetBooksModels.Success>($"/books/{data.Id}");
        Assert.NotNull(bookData);
        Assert.NotEqual(0, bookData.Id);
        
        var expected = new GetBooksModels.Success(
            data.Id,
            request.title,
            request.author,
            request.year,
            request.publisher,
            request.Description
        );
        
        Assert.Equal(bookData, expected);
    }
    
    
    [Fact]
    public async Task AddBook_OptionalFields()
    {
        var client = _webApplicationFactory.CreateClient();
        await DatabaseHelpers.EnsureMigrations(_webApplicationFactory.Services);
        await DatabaseHelpers.ClearBooks(_webApplicationFactory.Services);

        // add book
        
        var request = new
        {
            title = "A",
            author = "B",
            year = 2,
        };
        
        var response = await client.PostAsJsonAsync("/books", request);
        var data = await response.Content.ReadFromJsonAsync<AddBookModels.Success>();

        response.EnsureSuccessStatusCode();
        Assert.NotNull(data);
        Assert.NotEqual(0, data.Id);
        
        // ensure data is correct
        
        var bookData = await client.GetFromJsonAsync<GetBooksModels.Success>($"/books/{data.Id}");
        Assert.NotNull(bookData);
        Assert.NotEqual(0, bookData.Id);
        
        var expected = new GetBooksModels.Success(
            data.Id,
            request.title,
            request.author,
            request.year,
            null,
            null
        );
        
        Assert.Equal(bookData, expected);
    }
    
    [Fact]
    public async Task AddBook_StringsAreTrimmed()
    {
        var client = _webApplicationFactory.CreateClient();
        await DatabaseHelpers.EnsureMigrations(_webApplicationFactory.Services);
        await DatabaseHelpers.ClearBooks(_webApplicationFactory.Services);

        // add book 
        
        var request = new
        {
            title = " A ",
            author = "   B ",
            year = 2,
            publisher = " C  C ",
            Description = """
                          
                          D
                          
                          
                          
                          """
        };
        
        var response = await client.PostAsJsonAsync("/books", request);
        var data = await response.Content.ReadFromJsonAsync<AddBookModels.Success>();

        response.EnsureSuccessStatusCode();
        Assert.NotNull(data);
        Assert.NotEqual(0, data.Id);

        var bookData = await client.GetFromJsonAsync<GetBooksModels.Success>($"/books/{data.Id}");
        Assert.NotNull(bookData);
        Assert.NotEqual(0, bookData.Id);
        
        var expected = new GetBooksModels.Success(
            data!.Id,
            "A",
            "B",
            2,
            "C  C",
            "D"
        );
        
        Assert.Equal(bookData, expected);
    }
    
    [Fact]
    public async Task AddBook_MissingTitle()
    {
        var client = _webApplicationFactory.CreateClient();
        await DatabaseHelpers.EnsureMigrations(_webApplicationFactory.Services);
        await DatabaseHelpers.ClearBooks(_webApplicationFactory.Services);
        
        var request = new
        {
            author = "B",
            year = 2,
        };
        
        var response = await client.PostAsJsonAsync("/books", request);
        var data = await response.Content.ReadFromJsonAsync<AddBookModels.Failure>();

        Assert.Equal(400, (int)response.StatusCode);
        Assert.NotNull(data);
        
        Assert.Contains("missing required properties", data.Detail);
        Assert.Contains("title", data.Detail);
    }
    
    [Fact]
    public async Task AddBook_EmptyTitle()
    {
        var client = _webApplicationFactory.CreateClient();
        await DatabaseHelpers.EnsureMigrations(_webApplicationFactory.Services);
        await DatabaseHelpers.ClearBooks(_webApplicationFactory.Services);

        var request = new
        {
            title = "",
            author = "B",
            year = 2,
        };
        
        var response = await client.PostAsJsonAsync("/books", request);
        var data = await response.Content.ReadFromJsonAsync<AddBookModels.Failure>();

        Assert.Equal(400, (int)response.StatusCode);
        Assert.NotNull(data);
        Assert.Equal("Property must not be empty.", data.Errors["Title"].Single());
    }
    
    [Fact]
    public async Task AddBook_NullTitle()
    {
        var client = _webApplicationFactory.CreateClient();
        await DatabaseHelpers.EnsureMigrations(_webApplicationFactory.Services);
        await DatabaseHelpers.ClearBooks(_webApplicationFactory.Services);

        var request = new
        {
            title = null as string,
            author = "B",
            year = 2,
        };
        
        var response = await client.PostAsJsonAsync("/books", request);
        var data = await response.Content.ReadFromJsonAsync<AddBookModels.Failure>();

        Assert.Equal(400, (int)response.StatusCode);
        Assert.NotNull(data);
        Assert.Equal("Property must not be `null`.", data.Errors["Title"].Single());
    }
    
    [Fact]
    public async Task AddBook_Duplicate()
    {
        var client = _webApplicationFactory.CreateClient();
        await DatabaseHelpers.EnsureMigrations(_webApplicationFactory.Services);
        await DatabaseHelpers.ClearBooks(_webApplicationFactory.Services);

        var request = new
        {
            title = "A",
            author = "B",
            year = 2,
        };

        {
            // add first
            var response = await client.PostAsJsonAsync("/books", request);
            var data = await response.Content.ReadFromJsonAsync<AddBookModels.Success>();

            response.EnsureSuccessStatusCode();
            Assert.NotNull(data);
            Assert.NotEqual(0, data.Id);
        }

        {
            // add second
            var response = await client.PostAsJsonAsync("/books", request);
            var data = await response.Content.ReadFromJsonAsync<AddBookModels.Failure>();
            
            Assert.Equal(400, (int)response.StatusCode);
            Assert.NotNull(data);
            Assert.Equal("A book with the same title, author and year already exists", data.Detail);
            Assert.Equal("A book with the same title, author and year already exists", data.Errors["Model"].Single());
        }
    }
    
    [Fact]
    public async Task AddBook_EmptyPublisher()
    {
        var client = _webApplicationFactory.CreateClient();
        await DatabaseHelpers.EnsureMigrations(_webApplicationFactory.Services);
        await DatabaseHelpers.ClearBooks(_webApplicationFactory.Services);

        var request = new
        {
            title = "A",
            author = "B",
            year = 2,
            publisher = ""
        };
        
        var response = await client.PostAsJsonAsync("/books", request);
        var data = await response.Content.ReadFromJsonAsync<AddBookModels.Failure>();

        Assert.Equal(400, (int)response.StatusCode);
        Assert.NotNull(data);
        Assert.Equal("Property must not be empty.", data.Errors["Publisher"].Single());
    }
}