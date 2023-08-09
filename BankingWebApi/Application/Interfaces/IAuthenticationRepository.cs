using BankingWebApi.Application.Records;

namespace BankingWebApi.Application.Interfaces
{
    public interface IAuthenticationRepository
    {
        string CreateToken(AuthenticationRequestBody authenticationRequestBody);
    }
}
