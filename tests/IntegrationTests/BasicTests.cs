using Microsoft.AspNetCore.Mvc.Testing;
using Xunit.Abstractions;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace IntegrationTests;

public partial class BasicTests(WebApplicationFactory<Program> webApplicationFactory, ITestOutputHelper testOutputHelper) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _webApplicationFactory = webApplicationFactory;
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;

    [Fact]
    public async Task Get_Swagger_IsSuccess()
    {
        // Arrange
        var client = _webApplicationFactory.CreateClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("text/html", response.Content.Headers!.ContentType!.MediaType);
    }
}
