using Microsoft.AspNetCore.Mvc;
using static BankingWebApi.Repositories.AuthenticationRepository;

namespace BankingWebApi.Interfaces
{
    public interface IAuthenticationRepository
    {
        string CreateToken(AuthenticationRequestBody authenticationRequestBody);
    }
}
