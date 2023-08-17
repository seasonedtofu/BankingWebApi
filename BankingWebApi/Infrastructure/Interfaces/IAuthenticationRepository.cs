using BankingWebApi.Infrastructure.Records;

namespace BankingWebApi.Infrastructure.Interfaces
{
    public interface IAuthenticationRepository
    {
        string CreateToken(AuthenticationRequestBody authenticationRequestBody);
    }
}
