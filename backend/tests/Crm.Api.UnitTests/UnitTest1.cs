using Crm.Api.Options;
using Xunit;

namespace Crm.Api.UnitTests;

public class CorsOptionsTests
{
    [Fact]
    public void CorsOptions_DefaultValues_ShouldBeEmptyArray()
    {
        // Arrange & Act
        var options = new CorsOptions();

        // Assert
        Assert.NotNull(options.AllowedOrigins);
        Assert.Empty(options.AllowedOrigins);
    }
}