using System.Text.Json.Serialization;

namespace Connector;

using Json.Schema.Generation;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Contains configuration values necessary for the Pinecone connector execution.
/// </summary>
[Title("Connector Registration Configuration")]
[Description("Configuration settings for the Pinecone connector.")]
public class ConnectorRegistrationConfig
{
    /// <summary>
    /// Gets or sets the retry policy configuration.
    /// </summary>
    [Title("Retry Policy")]
    [Description("Configuration for HTTP request retry behavior.")]
    public RetryPolicyConfig RetryPolicy { get; set; } = new();

    /// <summary>
    /// Gets or sets the API client configuration.
    /// </summary>
    [Title("API Client")]
    [Description("Configuration for the Pinecone API client.")]
    [System.ComponentModel.DataAnnotations.Required]
    public ApiClientConfig ApiClient { get; set; } = new();

    [Title("Service Configuration")]
    [Description("Configuration for the service.")]
    [System.ComponentModel.DataAnnotations.Required]
    public ServiceConfig Service { get; set; } = new();
}

/// <summary>
/// Configuration for HTTP request retry behavior.
/// </summary>
public class RetryPolicyConfig
{
    /// <summary>
    /// Gets or sets the maximum number of retry attempts.
    /// </summary>
    [Description("Maximum number of retry attempts for failed requests.")]
    [Range(0, 5)]
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Gets or sets the initial retry delay in seconds.
    /// </summary>
    [Description("Initial delay between retry attempts in seconds.")]
    [Range(1, 30)]
    public int InitialRetryDelaySeconds { get; set; } = 2;

    /// <summary>
    /// Gets or sets whether to use exponential backoff.
    /// </summary>
    [Description("Whether to use exponential backoff for retry delays.")]
    public bool UseExponentialBackoff { get; set; } = true;
}

/// <summary>
/// Configuration for the Pinecone API client.
/// </summary>
public class ApiClientConfig
{
    /// <summary>
    /// Gets or sets the request timeout in seconds.
    /// </summary>
    [Description("Timeout for API requests in seconds.")]
    [Range(1, 300)]
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets whether to compress request content.
    /// </summary>
    [Description("Whether to compress request content when possible.")]
    public bool UseCompression { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum concurrent requests.
    /// </summary>
    [Description("Maximum number of concurrent API requests.")]
    [Range(1, 100)]
    public int MaxConcurrentRequests { get; set; } = 10;
}

public class ServiceConfig
{
    [Description("Default timeout for service operations in seconds.")]
    [Range(1, 300)]
    public int TimeoutSeconds { get; set; } = 30;

    [Description("Maximum number of concurrent operations.")]
    [Range(1, 100)]
    public int MaxConcurrentOperations { get; set; } = 10;
}
