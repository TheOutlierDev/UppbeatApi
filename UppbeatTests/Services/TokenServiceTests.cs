using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UppbeatApi.Services;

namespace UppbeatTests.Services;

public class TokenServiceTests
{
    private readonly TokenService _tokenService;
    private readonly IConfiguration _configuration;

    public TokenServiceTests()
    {
        // Setup configuration
        var inMemorySettings = new Dictionary<string, string> {
            {"Jwt:Key", "your-256-bit-secret-your-256-bit-secret-your-256-bit-secret"},
            {"Jwt:Issuer", "UppbeatLibraryAPI"},
            {"Jwt:Audience", "UppbeatLibraryAPI"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _tokenService = new TokenService(_configuration);
    }

    [Fact]
    public void GenerateToken_ValidInputs_ReturnsValidToken()
    {
        // Arrange
        var username = "testuser";
        var role = "Artist";

        // Act
        var token = _tokenService.GenerateToken(username, role);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        Assert.Equal(username, jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value);
        Assert.Equal(role, jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value);
        Assert.Equal(_configuration["Jwt:Issuer"], jwtToken.Issuer);
        Assert.Equal(_configuration["Jwt:Audience"], jwtToken.Audiences.First());
    }

    [Theory]
    [InlineData("", "Artist")]
    [InlineData("testuser", "")]
    [InlineData(null, "Artist")]
    [InlineData("testuser", null)]
    public void GenerateToken_InvalidInputs_ThrowsArgumentNullException(string username, string role)
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _tokenService.GenerateToken(username, role));
    }
}
