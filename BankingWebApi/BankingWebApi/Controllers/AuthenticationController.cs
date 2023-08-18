using BankingWebApi.Domain.Interfaces;
using BankingWebApi.Domain.Records;
using BankingWebApi.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BankingWebApi.Web.Controllers
{
    [Route("api/authentication")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly AuthenticationRepository _authenticationRepository;

        /// <summary>
        /// Authentication controller.
        /// </summary>
        /// <param name="authenticationRepository">Dependency injection for authentication repository</param>
        public AuthenticationController(IAuthenticationRepository authenticationRepository)
        {
            _authenticationRepository = (AuthenticationRepository?)authenticationRepository;
        }

        /// <summary>
        /// Creates a token to be used to authenticate.
        /// </summary>
        /// <returns>
        /// Returns an authentication token.
        /// </returns>
        /// <response code="200">Returns token.</response>
        [HttpPost("CreateToken")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public Task<ActionResult<string>> CreateToken(AuthenticationRequestBody authenticationRequestBody)
        {
            try
            {
                return Task.FromResult<ActionResult<string>>(_authenticationRepository.CreateToken(authenticationRequestBody));
            }
            catch (UnauthorizedAccessException e)
            {
                return Task.FromResult<ActionResult<string>>(Unauthorized(e.Message));
            }
            catch (Exception e)
            {
                return Task.FromResult<ActionResult<string>>(BadRequest(e.Message));
            }
        }
    }
}
