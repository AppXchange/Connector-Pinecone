using Xchange.Connector.SDK.Client.AuthTypes;
using System.Text.Json.Serialization;
using Json.Schema.Generation;
using System.ComponentModel.DataAnnotations;

namespace Connector.Client;

/// <summary>
/// Configuration for API key authentication.
/// </summary>
[Title("API Key Authentication")]
[Description("Authentication configuration using an API key.")]
public class ApiKeyAuth : IApiKeyAuth
{
    /// <summary>
    /// Gets or sets the API key for authentication.
    /// </summary>
    [System.ComponentModel.DataAnnotations.Required]
    [JsonPropertyName("apiKey")]
    [Description("The API key used for authentication with Pinecone.")]
    public string ApiKey { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the base URL for the API.
    /// </summary>
    [System.ComponentModel.DataAnnotations.Required]
    [JsonPropertyName("baseUrl")]
    [Description("The base URL for the Pinecone API.")]
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the environment identifier.
    /// </summary>
    [System.ComponentModel.DataAnnotations.Required]
    [JsonPropertyName("environment")]
    [Description("The Pinecone environment identifier.")]
    public string Environment { get; set; } = string.Empty;
} 