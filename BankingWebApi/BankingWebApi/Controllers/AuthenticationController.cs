using BankingWebApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace BankingWebApi.Controllers
{
    [Route("api/authentication")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly AuthenticationRepository _authenticationRepository;

        /// <summary>
        /// Authentication controller.
        /// </summary>
        /// <param name="authenticationRepository">Dependency injection for authenticatio repository</param>
        public AuthenticationController(AuthenticationRepository authenticationRepository)
        {
            _authenticationRepository = authenticationRepository;
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
        public Task<ActionResult<string>> CreateToken(AuthenticationRepository.AuthenticationRequestBody authenticationRequestBody)
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
