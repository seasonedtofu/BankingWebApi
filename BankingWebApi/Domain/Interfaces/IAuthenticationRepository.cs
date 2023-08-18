using BankingWebApi.Domain.Records;

namespace BankingWebApi.Domain.Interfaces
{
    public interface IAuthenticationRepository
    {
        string CreateToken(AuthenticationRequestBody authenticationRequestBody);
    }
}
