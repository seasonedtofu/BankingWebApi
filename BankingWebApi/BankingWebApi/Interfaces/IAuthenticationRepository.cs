using BankingWebApi.Records;

namespace BankingWebApi.Interfaces
{
    public interface IAuthenticationRepository
    {
        string CreateToken(AuthenticationRequestBody authenticationRequestBody);
    }
}
