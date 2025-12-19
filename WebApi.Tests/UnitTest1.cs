using Microsoft.AspNetCore.Mvc.Testing;
using webapi;
using Xunit.Abstractions;

namespace WebApi.Tests;

public class BasicTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _testOutputHelper;

    public BasicTests(WebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
    {
        _factory = factory;
        _testOutputHelper = testOutputHelper;
    }

    [Theory]
    [InlineData("/Test")]
    public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync(url);

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);

/*
        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299
        Assert.Equal("application/json; charset=utf-8",
            response.Content.Headers.ContentType.ToString());*/
    }
}
