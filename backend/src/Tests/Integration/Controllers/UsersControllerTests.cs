using System.Net;
using System.Net.Http.Json;
using Crm.Application.Common.Models;
using Crm.Application.Features.Users;
using Crm.Tests.Common;
using Gridify;
using Xunit;

namespace Crm.Tests.Integration.Controllers;

public class UsersControllerTests : IntegrationTestBase
{
    public UsersControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Create_ShouldReturnSuccess_WhenValid()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = $"test-{Guid.NewGuid()}@example.com",
            FullName = "Test User",
            Password = "Password123!",
            Title = "Mr"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Users", command);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<TestResult<long>>();
        Assert.True(result!.Succeeded);
        Assert.True(result.Data > 0);
    }

    [Fact]
    public async Task GetUser_ShouldReturnUser_WhenExists()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = $"test-{Guid.NewGuid()}@example.com",
            FullName = "Test User",
            Password = "Password123!"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/Users", command);
        var id = (await createResponse.Content.ReadFromJsonAsync<TestResult<long>>())!.Data;

        // Act
        var response = await _client.GetAsync($"/api/v1/Users/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<TestResult<UserDto>>();
        Assert.True(result!.Succeeded);
        Assert.Equal(command.Email, result.Data!.Email);
    }

    [Fact]
    public async Task Delete_ShouldReturnSuccess_WhenExists()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = $"test-{Guid.NewGuid()}@example.com",
            FullName = "To Delete",
            Password = "Password123!"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/Users", command);
        var id = (await createResponse.Content.ReadFromJsonAsync<TestResult<long>>())!.Data;

        // Act
        var response = await _client.DeleteAsync($"/api/v1/Users/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<TestResult<bool>>();
        Assert.True(result!.Succeeded);

        // Verify it's gone
        var getResponse = await _client.GetAsync($"/api/v1/Users/{id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetUsers_ShouldReturnPagedList()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Users?Page=1&PageSize=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<TestResult<Paging<UserDto>>>();
        Assert.True(result!.Succeeded);
        Assert.NotNull(result.Data);
    }
}
