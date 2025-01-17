using System;
using System.Net;

namespace Connector.Client;

/// <summary>
/// Represents errors that occur during API operations with the Pinecone service.
/// </summary>
public class ApiException : Exception
{
    /// <summary>
    /// Gets the HTTP status code associated with the API error.
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Gets the raw response content from the API if available.
    /// </summary>
    public string? ResponseContent { get; }

    /// <summary>
    /// Initializes a new instance of the ApiException class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ApiException(string message) : base(message)
    {
        StatusCode = HttpStatusCode.InternalServerError;
    }

    /// <summary>
    /// Initializes a new instance of the ApiException class with a status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="message">The error message.</param>
    public ApiException(HttpStatusCode statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Initializes a new instance of the ApiException class with a status code and response content.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="responseContent">The raw response content.</param>
    public ApiException(HttpStatusCode statusCode, string message, string? responseContent) : base(message)
    {
        StatusCode = statusCode;
        ResponseContent = responseContent;
    }

    /// <summary>
    /// Initializes a new instance of the ApiException class with a message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ApiException(string message, Exception innerException) : base(message, innerException)
    {
        StatusCode = HttpStatusCode.InternalServerError;
    }

    /// <summary>
    /// Creates an ApiException based on the HTTP status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="responseContent">Optional response content.</param>
    /// <returns>An appropriate ApiException instance.</returns>
    public static ApiException FromStatusCode(HttpStatusCode statusCode, string? responseContent = null)
    {
        var message = statusCode switch
        {
            HttpStatusCode.Unauthorized => "Authentication failed. Please check your API key.",
            HttpStatusCode.Forbidden => "Access denied. Your API key may not have the required permissions.",
            HttpStatusCode.NotFound => "The requested resource was not found.",
            HttpStatusCode.BadRequest => "The request was invalid or malformed.",
            HttpStatusCode.TooManyRequests => "Rate limit exceeded. Please try again later.",
            _ => $"API request failed with status code {statusCode}"
        };

        return new ApiException(statusCode, message, responseContent);
    }
}