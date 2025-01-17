using Connector.Client;
using System;
using ESR.Hosting.CacheWriter;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Xchange.Connector.SDK.CacheWriter;
using System.Net.Http;
using System.Text.Json;
using Xchange.Connector.SDK.Abstraction;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Net.Http.Json;

namespace Connector.App.v1.Index;

public class IndexDataReader : IAsyncDataReader<IndexDataObject>
{
    private readonly ApiClient _apiClient;
    private readonly AppV1CacheWriterConfig _config;

    public IndexDataReader(ApiClient apiClient, AppV1CacheWriterConfig config)
    {
        _apiClient = apiClient;
        _config = config;
    }

    async IAsyncEnumerable<object> IAsyncDataReader.GetDataAsync(
        DataObjectCacheWriteArguments? dataObjectRunArguments,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Task.Yield();
        foreach (var item in GetData(dataObjectRunArguments, cancellationToken))
        {
            yield return item;
        }
    }

    public async IAsyncEnumerable<IndexDataObject> GetTypedDataAsync(
        DataObjectCacheWriteArguments? dataObjectRunArguments,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Task.Yield();
        foreach (var item in GetData(dataObjectRunArguments, cancellationToken))
        {
            yield return item;
        }
    }

    public IEnumerable<IndexDataObject> GetData(
        DataObjectCacheWriteArguments? dataObjectRunArguments,
        CancellationToken cancellationToken)
    {
        var response = _apiClient.GetAsync("indexes", cancellationToken).Result;
        if (!response.IsSuccessStatusCode)
        {
            yield break;
        }

        var indexList = response.Content.ReadFromJsonAsync<IndexListResponse>(_apiClient.JsonOptions, cancellationToken).Result;
        if (indexList?.Indexes == null)
        {
            yield break;
        }

        foreach (var index in indexList.Indexes)
        {
            yield return new IndexDataObject
            {
                Id = index.Name,
                Name = index.Name,
                Dimension = index.Dimension,
                Metric = Enum.Parse<MetricType>(index.Metric, true),
                Host = index.Host,
                Spec = index.Spec
            };
        }
    }
}

public class IndexListResponse
{
    [JsonPropertyName("indexes")]
    public List<IndexInfo> Indexes { get; set; } = new();
}

public class IndexInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("dimension")]
    public int Dimension { get; set; }

    [JsonPropertyName("metric")]
    public string Metric { get; set; } = string.Empty;

    [JsonPropertyName("host")]
    public string Host { get; set; } = string.Empty;

    [JsonPropertyName("spec")]
    public IndexSpec Spec { get; set; } = new();
}