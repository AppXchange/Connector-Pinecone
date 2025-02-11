using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Connector.App.v1.Vector;
using Connector.App.v1.Index;
using Connector.App.v1.Embed;
using System.Linq;
using System.IO;
using System.Text.Json.Nodes;
using Connector.App.v1;
using System.Text.Json.Serialization;

namespace Connector.Client;

/// <summary>
/// A client for interfacing with the Pinecone API.
/// </summary>
public class ApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ActionProcessor _actionProcessor;
    public JsonSerializerOptions JsonOptions { get; }

    public ApiClient(IHttpClientFactory httpClientFactory, ActionProcessor actionProcessor)
    {
        _httpClientFactory = httpClientFactory;
        _actionProcessor = actionProcessor;
        
        JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    private HttpClient CreateClient(string baseUrl, string apiKey)
    {
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(baseUrl);
        client.DefaultRequestHeaders.Add("Api-Key", apiKey);
        client.DefaultRequestHeaders.Add("X-Pinecone-API-Version", "2024-10");
        return client;
    }

    // Vector Operations
    public async Task<ApiResponse<VectorDataObject>> UpsertVector(VectorDataObject vector, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(
            _actionProcessor.Vector.IndexHost,
            _actionProcessor.Vector.ApiKey
        );

        var request = new
        {
            vectors = new[]
            {
                new
                {
                    id = vector.Id,
                    values = vector.Values,
                    sparseValues = vector.SparseValues,
                    metadata = vector.Metadata,
                    nameSpace = vector.Namespace
                }
            }
        };

        var response = await client.PostAsJsonAsync("vectors/upsert", request, JsonOptions, cancellationToken);
        
        return new ApiResponse<VectorDataObject>
        {
            IsSuccessful = response.IsSuccessStatusCode,
            StatusCode = response.StatusCode,
            Data = response.IsSuccessStatusCode ? vector : default,
            RawResult = await response.Content.ReadAsStreamAsync(cancellationToken)
        };
    }

    // Index Operations
    public async Task<ApiResponse<IndexDataObject>> CreateIndex(IndexDataObject index, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(
            _actionProcessor.Index.BaseUrl,
            _actionProcessor.Index.ApiKey
        );

        var request = new
        {
            name = index.Name,
            dimension = index.Dimension,
            metric = index.Metric.ToString().ToLowerInvariant(),
            spec = index.Spec,
            deletionProtection = index.DeletionProtection
        };

        Console.WriteLine($"Request Body: {JsonSerializer.Serialize(request, JsonOptions)}");

        var response = await client.PostAsJsonAsync("indexes", request, JsonOptions, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Error creating index: {content}");
            Console.WriteLine($"Request headers: {string.Join(", ", client.DefaultRequestHeaders)}");
        }

        return new ApiResponse<IndexDataObject>
        {
            IsSuccessful = response.IsSuccessStatusCode,
            StatusCode = response.StatusCode,
            Data = response.IsSuccessStatusCode ? index : default,
            ErrorMessage = !response.IsSuccessStatusCode ? content : null,
            RawResult = await response.Content.ReadAsStreamAsync(cancellationToken)
        };
    }

    public async Task<ApiResponse> DeleteIndex(string indexName, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(
            _actionProcessor.Index.BaseUrl,
            _actionProcessor.Index.ApiKey
        );
        var response = await client.DeleteAsync($"databases/{indexName}", cancellationToken);

        return new ApiResponse
        {
            IsSuccessful = response.IsSuccessStatusCode,
            StatusCode = response.StatusCode,
            RawResult = await response.Content.ReadAsStreamAsync(cancellationToken)
        };
    }

    // Embed Operations
    public async Task<ApiResponse<EmbedDataObject>> GenerateEmbeddings(EmbedDataObject embed, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(
            _actionProcessor.Embed.BaseUrl,
            _actionProcessor.Embed.ApiKey
        );

        var request = new
        {
            inputs = embed.Inputs.Select(i => new { text = i.Text }).ToArray(),
            model = embed.Model,
            parameters = embed.Parameters
        };
        
        Console.WriteLine($"Request URL: {client.BaseAddress}embed");
        Console.WriteLine($"Request Headers: {string.Join(", ", client.DefaultRequestHeaders.Select(h => $"{h.Key}={string.Join(",", h.Value)}"))}");
        Console.WriteLine($"Request Body: {JsonSerializer.Serialize(request, JsonOptions)}");

        var response = await client.PostAsJsonAsync("embed", request, JsonOptions, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        Console.WriteLine($"Response Status: {response.StatusCode}");
        Console.WriteLine($"Response Content: {content}");

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Error Status Code: {response.StatusCode}");
            Console.WriteLine($"Error Response: {content}");
        }

        return new ApiResponse<EmbedDataObject>
        {
            IsSuccessful = response.IsSuccessStatusCode,
            StatusCode = response.StatusCode,
            Data = response.IsSuccessStatusCode ? embed : default,
            ErrorMessage = !response.IsSuccessStatusCode ? content : null,
            RawResult = await response.Content.ReadAsStreamAsync(cancellationToken)
        };
    }

    public async Task<ApiResponse> TestConnection(CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(
            _actionProcessor.Index.BaseUrl,
            _actionProcessor.Index.ApiKey
        );
        var response = await client.GetAsync("describe-index-stats", cancellationToken);

        return new ApiResponse
        {
            IsSuccessful = response.IsSuccessStatusCode,
            StatusCode = response.StatusCode,
            RawResult = await response.Content.ReadAsStreamAsync(cancellationToken)
        };
    }

    public async Task<ApiResponse<List<IndexDataObject>>> GetIndexes(CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(
            _actionProcessor.Index.BaseUrl,
            _actionProcessor.Index.ApiKey
        );
        var response = await client.GetAsync("indexes", cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            return new ApiResponse<List<IndexDataObject>>
            {
                IsSuccessful = false,
                StatusCode = response.StatusCode,
                ErrorMessage = await response.Content.ReadAsStringAsync(cancellationToken)
            };
        }

        var indexList = await response.Content.ReadFromJsonAsync<IndexListResponse>(JsonOptions, cancellationToken);
        var indexes = indexList?.Indexes.Select(i => new IndexDataObject
        {
            Id = i.Name,
            Name = i.Name,
            Dimension = i.Dimension,
            Metric = Enum.Parse<App.v1.Index.MetricType>(i.Metric, true),
            Host = i.Host,
            Spec = i.Spec
        }).ToList() ?? new List<IndexDataObject>();

        return new ApiResponse<List<IndexDataObject>>
        {
            IsSuccessful = true,
            StatusCode = response.StatusCode,
            Data = indexes
        };
    }

    public async Task<ApiResponse<List<VectorDataObject>>> FetchVectors(CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(
            _actionProcessor.Index.BaseUrl,
            _actionProcessor.Index.ApiKey
        );
        var response = await client.PostAsync("/vectors/fetch", null, cancellationToken);
        return new ApiResponse<List<VectorDataObject>>
        {
            IsSuccessful = response.IsSuccessStatusCode,
            StatusCode = (System.Net.HttpStatusCode)(int)response.StatusCode,
            Data = response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<List<VectorDataObject>>(cancellationToken) : new List<VectorDataObject>()
        };
    }

    public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(
            _actionProcessor.Index.BaseUrl,
            _actionProcessor.Index.ApiKey
        );
        var response = await client.PostAsync(requestUri, content, cancellationToken);
        return response;
    }

    public async Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(
            _actionProcessor.Index.BaseUrl,
            _actionProcessor.Index.ApiKey
        );
        var response = await client.GetAsync(requestUri, cancellationToken);
        return response;
    }
}

public class TestSettings
{
    [JsonPropertyName("testSettings")]
    public TestSettingsData Settings { get; set; } = new();
}

public class TestSettingsData
{
    [JsonPropertyName("activeConnection")]
    public string ActiveConnection { get; set; } = string.Empty;
    
    public List<ConnectionConfig> Connections { get; set; } = new();
    
    [JsonPropertyName("app-1")]
    public AppConfig? App1 { get; set; }

    // Add this to match the JSON structure
    [JsonPropertyName("connectorRegistration")]
    public ConnectorRegistration? ConnectorRegistration { get; set; }
}

public class ConnectionConfig
{
    public string DefinitionKey { get; set; } = string.Empty;
    public JsonElement Configuration { get; set; }
}

public class AppConfig
{
    public ServiceConfiguration ServiceConfiguration { get; set; } = new();
}

public class ServiceConfiguration
{
    [JsonPropertyName("actionProcessor")]
    public ActionProcessor ActionProcessor { get; set; } = new();
}

public class ActionProcessor
{
    [JsonPropertyName("Vector")]
    public VectorConfig Vector { get; set; } = new();

    [JsonPropertyName("Index")]
    public IndexConfig Index { get; set; } = new();

    [JsonPropertyName("Embed")]
    public EmbedConfig Embed { get; set; } = new();
}

public class VectorConfig
{
    [JsonPropertyName("IndexHost")]
    public string IndexHost { get; set; } = string.Empty;

    [JsonPropertyName("ApiKey")]
    public string ApiKey { get; set; } = string.Empty;
}

public class IndexConfig
{
    [JsonPropertyName("BaseUrl")]
    public string BaseUrl { get; set; } = string.Empty;

    [JsonPropertyName("ApiKey")]
    public string ApiKey { get; set; } = string.Empty;
}

public class EmbedConfig
{
    [JsonPropertyName("ApiKey")]
    public string ApiKey { get; set; } = string.Empty;

    [JsonPropertyName("BaseUrl")]
    public string BaseUrl { get; set; } = string.Empty;

    [JsonPropertyName("DefaultModel")]
    public string DefaultModel { get; set; } = string.Empty;

    [JsonPropertyName("MaxBatchSize")]
    public int MaxBatchSize { get; set; }
}

// Add this class to match the JSON structure
public class ConnectorRegistration
{
    [JsonPropertyName("RetryPolicy")]
    public RetryPolicy? RetryPolicy { get; set; }

    [JsonPropertyName("ApiClient")]
    public ApiClientConfig? ApiClient { get; set; }

    [JsonPropertyName("Service")]
    public ConnectorServiceConfig? Service { get; set; }
}

// Add these classes after the existing ones
public class RetryPolicy
{
    public int MaxRetries { get; set; }
    public int InitialRetryDelaySeconds { get; set; }
    public bool UseExponentialBackoff { get; set; }
}

public class ApiClientConfig
{
    public int TimeoutSeconds { get; set; }
    public bool UseCompression { get; set; }
    public int MaxConcurrentRequests { get; set; }
}

public class ConnectorServiceConfig
{
    public int TimeoutSeconds { get; set; }
    public int MaxConcurrentOperations { get; set; }
}