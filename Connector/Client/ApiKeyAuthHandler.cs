using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Xchange.Connector.SDK.Client.AuthTypes;

namespace Connector.Client;

/// <summary>
/// Handles API key authentication for HTTP requests to the Pinecone API.
/// </summary>
public class ApiKeyAuthHandler : DelegatingHandler
{
    private readonly IApiKeyAuth _apiKeyAuth;

    /// <summary>
    /// Initializes a new instance of the ApiKeyAuthHandler class.
    /// </summary>
    /// <param name="apiKeyAuth">The API key authentication configuration.</param>
    public ApiKeyAuthHandler(IApiKeyAuth apiKeyAuth)
    {
        _apiKeyAuth = apiKeyAuth;
    }

    /// <summary>
    /// Adds the API key authentication header to outgoing requests.
    /// </summary>
    /// <param name="request">The HTTP request message.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The HTTP response message.</returns>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_apiKeyAuth.ApiKey))
        {
            throw new System.InvalidOperationException("API key is not configured");
        }

        // Add API key to header
        request.Headers.Add("Api-Key", _apiKeyAuth.ApiKey);

        // Add common headers for Pinecone API
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Add("Accept-Encoding", "gzip, deflate");
        
        if (request.Content != null)
        {
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        }

        try
        {
            return await base.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw ApiException.FromStatusCode(ex.StatusCode ?? System.Net.HttpStatusCode.InternalServerError);
        }
    }
}