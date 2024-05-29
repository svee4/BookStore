using IntegrationTests.Helpers;

namespace IntegrationTests;

public partial class BasicTests
{
    [Fact]
    public async Task DeleteBook_IsSuccess()
    {
        var client = _webApplicationFactory.CreateClient();
        await DatabaseHelpers.EnsureMigrations(_webApplicationFactory.Services);
        await DatabaseHelpers.ClearBooks(_webApplicationFactory.Services);

        // create
        
        var request = new
        {
            title = "A",
            author = "B",
            year = 2,
        };

        int id = 0;
        
        {
            var response = await client.PostAsJsonAsync("/books", request);
            var data = await response.Content.ReadFromJsonAsync<AddBookModels.Success>();

            response.EnsureSuccessStatusCode();
            Assert.NotNull(data);
            Assert.NotEqual(0, data.Id);
            id = data.Id;
        }

        // delete

        {
            var response = await client.DeleteAsync($"/books/{id}");
            
            Assert.Equal(204, (int)response.StatusCode);
        }
        
        // ensure deleted

        {
            var response = await client.GetAsync($"/books/{id}");
            Assert.Equal(404, (int)response.StatusCode);
        }
    }
}