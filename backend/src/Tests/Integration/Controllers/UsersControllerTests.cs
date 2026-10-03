using System.Net;
using System.Net.Http.Json;
using Crm.Application.Common.Models;
using Crm.Application.Features.Users;
using Crm.Tests.Common;
using FluentAssertions;
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
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TestResult<long>>();
        result!.Succeeded.Should().BeTrue();
        result.Data.Should().BeGreaterThan(0);
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
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TestResult<UserDto>>();
        result!.Succeeded.Should().BeTrue();
        result.Data!.Email.Should().Be(command.Email);
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
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TestResult<bool>>();
        result!.Succeeded.Should().BeTrue();

        // Verify it's gone
        var getResponse = await _client.GetAsync($"/api/v1/Users/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUsers_ShouldReturnPagedList()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Users?Page=1&PageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TestResult<Paging<UserDto>>>();
        result!.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }
}
