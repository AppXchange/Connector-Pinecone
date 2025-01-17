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

namespace Connector.App.v1.Embed;

public class EmbedDataReader : IAsyncDataReader<EmbedDataObject>
{
    private readonly ApiClient _apiClient;
    private readonly AppV1CacheWriterConfig _config;

    public EmbedDataReader(ApiClient apiClient, AppV1CacheWriterConfig config)
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

    public async IAsyncEnumerable<EmbedDataObject> GetTypedDataAsync(
        DataObjectCacheWriteArguments? dataObjectRunArguments,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Task.Yield();
        foreach (var item in GetData(dataObjectRunArguments, cancellationToken))
        {
            yield return item;
        }
    }

    public IEnumerable<EmbedDataObject> GetData(
        DataObjectCacheWriteArguments? dataObjectRunArguments,
        CancellationToken cancellationToken)
    {
        // Implementation...
        yield break;
    }
}