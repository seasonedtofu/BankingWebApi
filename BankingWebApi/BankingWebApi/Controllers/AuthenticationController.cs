using Microsoft.AspNetCore.Mvc;

namespace BankingWebApi.Controllers
{
    [Route("api/authentication")]
    [ApiController]
    public class AuthenticationController
    {
        public class AuthenticationRequestBody : ControllerBase
        {

        }

        [HttpPost("authenticate")]
        public ActionResult<string> Authenticate(AuthenticationRequestBody authenticationRequestBody)
        {
            //var user = ValidateUserCredentials();
            return "test";
        }
    }
}
