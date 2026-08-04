using AssignmentSystem.Api.Models;
using AssignmentSystem.Api.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace AssignmentSystem.Tests;

public class AuthTests
{
    private JwtService BuildJwtService()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-secret-key-at-least-32-characters-long-for-testing",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:ExpiryMinutes"] = "60"
            })
            .Build();

        return new JwtService(config);
    }

    [Fact]
    public void HashPassword_ThenVerify_Succeeds()
    {
        var plainPassword = "MySecret@123";
        var hash = BCrypt.Net.BCrypt.HashPassword(plainPassword);

        Assert.True(BCrypt.Net.BCrypt.Verify(plainPassword, hash));
    }

    [Fact]
    public void VerifyPassword_WithWrongPassword_Fails()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword");

        Assert.False(BCrypt.Net.BCrypt.Verify("WrongPassword", hash));
    }

    [Fact]
    public void GenerateToken_IncludesCorrectRoleClaim()
    {
        var jwtService = BuildJwtService();
        var user = new User { Id = 1, FullName = "Test Teacher", Email = "t@test.com", Role = UserRole.Teacher };

        var token = jwtService.GenerateToken(user);
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var roleClaim = jwt.Claims.First(c => c.Type == System.Security.Claims.ClaimTypes.Role);
        Assert.Equal("Teacher", roleClaim.Value);
    }
}