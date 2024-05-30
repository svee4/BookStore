using Microsoft.AspNetCore.Mvc.Testing;
using Xunit.Abstractions;

namespace IntegrationTests;

public partial class BasicTests(
        WebApplicationFactory<Program> webApplicationFactory, 
        ITestOutputHelper testOutputHelper) 
    : IClassFixture<WebApplicationFactory<Program>>
{
    
    private readonly WebApplicationFactory<Program> _webApplicationFactory = webApplicationFactory;
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;

    [Fact]
    public async Task Get_Swagger_IsSuccess()
    {
        var client = _webApplicationFactory.CreateClient();

        var response = await client.GetAsync("/");

        response.EnsureSuccessStatusCode();
        Assert.Equal("text/html", response.Content.Headers!.ContentType!.MediaType);
    }
}
