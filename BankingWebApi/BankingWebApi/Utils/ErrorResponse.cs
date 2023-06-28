using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BankingWebApi.Utils;

public static class ErrorResponse
{
    public static ObjectResult Response(string message, HttpStatusCode statusCode)
    {
        return new ObjectResult(message) { StatusCode = (int)statusCode };
    }
}
