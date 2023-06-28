using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BankingWebApi.Interfaces;

public interface IErrorMessages
{
    ObjectResult Response(string message, HttpStatusCode statusCode);

    ObjectResult NotFound(HttpStatusCode statusCode = HttpStatusCode.NotFound);
}
