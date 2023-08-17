using BankingWebApi.Infrastructure.Records;
using BankingWebApi.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System.IdentityModel.Tokens.Jwt;
using Xunit;

namespace BankingWebApi.Infrastructure.Tests
{
    sealed class MockConfiguration: IConfiguration
    {
        private readonly Dictionary<string, string?> _dict;

        public string? this[string key]
        {
            get => _dict[key];
            set => _dict[key] = value;
        }

        public MockConfiguration()
        {
            _dict = new()
            {
                { "Authentication:SecretForKey", "thisisthesecretforgeneratingakey(mustbeatleast32bitlong)" },
                { "Authentication:Issuer", "TestingIssuer" },
                { "Authentication:Audience", "TestingAudience" },
            };
        }

        public IConfigurationSection GetSection(string key) => null;

        public IEnumerable<IConfigurationSection> GetChildren() => new IConfigurationSection[0];

        public IChangeToken GetReloadToken() => null;
    }

    public class TestAuthenticationRepository
    {
        private AuthenticationRepository _repository = new AuthenticationRepository(new MockConfiguration());
        public TestAuthenticationRepository() {}

        [Fact]
        public void CreateTokenGenerateJWTString()
        {
            var authRequest = new AuthenticationRequestBody("TestUserName", "TestPassword");
            var jwtToken = _repository.CreateToken(authRequest);
            var jwtIsEmpty = string.IsNullOrEmpty(jwtToken);

            Assert.False(jwtIsEmpty);
            Assert.True(new JwtSecurityTokenHandler().CanReadToken(jwtToken));
        }
    }
}
