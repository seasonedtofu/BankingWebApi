using BankingWebApi.Infrastructure.Interfaces;
using BankingWebApi.Infrastructure.Records;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BankingWebApi.Infrastructure.Repositories
{
    public class AuthenticationRepository : IAuthenticationRepository
    {
        private IConfiguration _configuration;

        public AuthenticationRepository(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public string CreateToken(AuthenticationRequestBody authenticationRequestBody)
        {
            // FOR PROJECT PURPOSES ONLY WE ASSUME USER AND PW IS CORRECT AND DO NOT ACTUALLY VALIDATE
            // In an actual implementation, we would call a function to authenticate the user with the authenticationRequestBody as a
            // parameter and then return a UserInfo.
            var user = new UserInfo(Guid.NewGuid(), authenticationRequestBody.UserName ?? "");

            if (user == null)
            {
                throw new UnauthorizedAccessException();
            }

            var securityKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(_configuration["Authentication:SecretForKey"]));
            var signingCredentials = new SigningCredentials(
                securityKey, SecurityAlgorithms.HmacSha256);
            var claimsForToken = new List<Claim>();
            claimsForToken.Add(new Claim("sub", user.UserId.ToString()));
            claimsForToken.Add(new Claim("user_name", user.UserName.ToString()));

            var jwtSecurityToken = new JwtSecurityToken(
                _configuration["Authentication:Issuer"],
                _configuration["Authentication:Audience"],
                claimsForToken,
                DateTime.UtcNow,
                DateTime.UtcNow.AddHours(1),
                signingCredentials);

            var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

            return token;
        }
    }
}
