using System.IO;
using System.Net;
using System.Text.Json.Serialization;

namespace Connector.Client;

/// <summary>
/// Represents a base response from the Pinecone API.
/// </summary>
public class ApiResponse
{
    /// <summary>
    /// Gets or initializes whether the API request was successful.
    /// </summary>
    public bool IsSuccessful { get; init; }

    /// <summary>
    /// Gets or initializes the HTTP status code of the response.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public HttpStatusCode StatusCode { get; init; }

    /// <summary>
    /// Gets or initializes the raw response content.
    /// Only available when Data is null.
    /// </summary>
    public Stream? RawResult { get; init; }

    /// <summary>
    /// Gets or initializes any error message from the API.
    /// </summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Represents a typed response from the Pinecone API.
/// </summary>
/// <typeparam name="TResult">The type of the response data.</typeparam>
public class ApiResponse<TResult> : ApiResponse
{
    /// <summary>
    /// Gets or initializes the typed response data.
    /// Only available when the request is successful.
    /// </summary>
    public TResult? Data { get; init; }

    /// <summary>
    /// Creates a successful response with data.
    /// </summary>
    /// <param name="data">The response data.</param>
    /// <returns>A successful API response.</returns>
    public static ApiResponse<TResult> Success(TResult data)
    {
        return new ApiResponse<TResult>
        {
            IsSuccessful = true,
            StatusCode = HttpStatusCode.OK,
            Data = data
        };
    }

    /// <summary>
    /// Creates a failed response with an error message.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="errorMessage">The error message.</param>
    /// <returns>A failed API response.</returns>
    public static ApiResponse<TResult> Failure(HttpStatusCode statusCode, string errorMessage)
    {
        return new ApiResponse<TResult>
        {
            IsSuccessful = false,
            StatusCode = statusCode,
            ErrorMessage = errorMessage,
            Data = default
        };
    }
}