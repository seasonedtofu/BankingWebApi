using System.IdentityModel.Tokens.Jwt;
using BankingWebApi.Application.Records;
using BankingWebApi.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Application.UnitTests
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

    public class AuthenticationRespositoryTests
    {
        private AuthenticationRepository _repository = new AuthenticationRepository(new MockConfiguration());
        public AuthenticationRespositoryTests() {}

        [Fact]
        public void CreateToken_Generate_JWT_String()
        {
            var authRequest = new AuthenticationRequestBody("TestUserName", "TestPassword");
            var jwtToken = _repository.CreateToken(authRequest);
            var jwtIsEmpty = string.IsNullOrEmpty(jwtToken);
            Assert.False(jwtIsEmpty);
            Assert.True(new JwtSecurityTokenHandler().CanReadToken(jwtToken));
        }
    }
}
