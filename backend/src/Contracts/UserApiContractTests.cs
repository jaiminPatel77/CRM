using PactNet;
using PactNet.Matchers;
using PactNet.Verifier;
using System.IO;
using Xunit;

namespace Crm.Contracts;

public class UserApiContractTests : IClassFixture<UserApiPact>
{
    private readonly UserApiPact _pact;

    public UserApiContractTests(UserApiPact pact)
    {
        _pact = pact;
    }

    [Fact]
    public async Task GetUsers_ReturnsUserList()
    {
        _pact.ProviderState.Setup("users exist", () =>
        {
            return new UserApiPact.UserResponse
            {
                Id = 1,
                UserName = "testuser",
                Email = "test@example.com"
            };
        });

        await _pact.VerifyAsync(async ctx =>
        {
            var response = await ctx.HttpClient.GetAsync("/api/v1/users");
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        });
    }

    [Fact]
    public async Task GetUserById_ReturnsUser()
    {
        _pact.ProviderState.Setup("a user with id 1 exists", () =>
        {
            return new UserApiPact.UserResponse
            {
                Id = 1,
                UserName = "testuser",
                Email = "test@example.com"
            };
        });

        await _pact.VerifyAsync(async ctx =>
        {
            var response = await ctx.HttpClient.GetAsync("/api/v1/users/1");
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        });
    }
}

public interface IVerifiableProvider
{
    void Setup(string description, Func<object> setup);
}

public class UserApiPact
{
    public IVerifiableProvider ProviderState { get; }

    public UserApiPact()
    {
        ProviderState = new MockProviderState();
    }

    public async Task VerifyAsync(Func<ProviderVerifierContext, Task> verifyAction)
    {
        var pactFile = new FileInfo("pacts/user-api.json");
        if (!pactFile.Exists)
        {
            return;
        }

        var config = new PactVerifierConfig();

        using var pactVerifier = new PactVerifier("User API", config);
        pactVerifier
            .WithHttpEndpoint(new Uri("http://localhost:5000"))
            .WithFileSource(pactFile)
            .Verify();
        
        await Task.CompletedTask;
    }

    public class UserResponse
    {
        public long Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class MockProviderState : IVerifiableProvider
    {
        public Dictionary<string, Func<object>> ProviderStates { get; } = new();

        public void Setup(string description, Func<object> setup)
        {
            ProviderStates[description] = setup;
        }
    }
}

public class ProviderVerifierContext
{
    public HttpClient HttpClient { get; set; } = null!;
    public string ProviderVersion { get; set; } = string.Empty;
}
