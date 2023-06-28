using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BankingWebApi.Utils;

/// <summary>
/// Custom response body for error handling.
/// </summary>
public class ErrorResponse: BankingWebApi.Interfaces.IErrorMessages
{
    /// <summary>
    /// Sends a custom response body.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="statusCode"></param>
    /// <returns>
    /// Custom response body with provided message & status code.
    /// </returns>
    public ObjectResult Response(string message, HttpStatusCode statusCode)
    {
        return new ObjectResult(message) { StatusCode = (int)statusCode };
    }

    /// <summary>
    /// Generic account not found response body.
    /// </summary>
    /// <param name="statusCode"></param>
    /// <returns>
    /// Custom response body with generic account not found with provided GUID & provided status code.
    /// </returns>
    public ObjectResult NotFound(HttpStatusCode statusCode = HttpStatusCode.NotFound)
    {
        return Response("Could not find account with provided GUID.", statusCode);
    }
}
