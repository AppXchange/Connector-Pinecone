using Connector.Client;
using ESR.Hosting.Action;
using ESR.Hosting.CacheWriter;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xchange.Connector.SDK.Action;
using Xchange.Connector.SDK.CacheWriter;
using Xchange.Connector.SDK.Client.AppNetwork;
using System.Linq;

namespace Connector.App.v1.Embed.Create;

public class CreateEmbedHandler : IActionHandler<CreateEmbedAction>
{
    private readonly ILogger<CreateEmbedHandler> _logger;
    private readonly ApiClient _apiClient;
    private readonly AppV1ActionProcessorConfig _config;

    public CreateEmbedHandler(
        ILogger<CreateEmbedHandler> logger,
        ApiClient apiClient,
        AppV1ActionProcessorConfig config)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }
    
    public async Task<ActionHandlerOutcome> HandleQueuedActionAsync(
        ActionInstance actionInstance, 
        CancellationToken cancellationToken)
    {
        var input = JsonSerializer.Deserialize<CreateEmbedActionInput>(
            actionInstance.InputJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (input == null)
        {
            throw new ArgumentException("Failed to deserialize action input", nameof(actionInstance));
        }

        try
        {
            _logger.LogInformation("Generating embeddings for {Count} inputs", input.Inputs.Length);

            var embedObject = new EmbedDataObject
            {
                Id = Guid.NewGuid().ToString(),
                Inputs = input.Inputs.Select(i => new Embed.EmbedInput { Text = i.Text }).ToList(),
                Model = input.Model ?? _config.Embed.DefaultModel,
                Parameters = input.Parameters ?? new Parameters()
            };

            var response = await _apiClient.GenerateEmbeddings(embedObject, cancellationToken);

            if (!response.IsSuccessful)
            {
                return ActionHandlerOutcome.Failed(new StandardActionFailure
                {
                    Code = response.StatusCode.ToString(),
                    Errors = new[]
                    {
                        new Error
                        {
                            Source = new[] { "CreateEmbedHandler" },
                            Text = response.ErrorMessage ?? "Failed to generate embeddings"
                        }
                    }
                });
            }

            var output = new CreateEmbedActionOutput
            {
                Embeddings = response.Data!.Inputs.Select(input => new float[0]).ToArray(),
                Model = response.Data.Model
            };

            // Update cache
            var operations = new List<SyncOperation>();
            var keyResolver = new DefaultDataObjectKey();
            var key = keyResolver.BuildKeyResolver()(response.Data);
            operations.Add(SyncOperation.CreateSyncOperation(
                UpdateOperation.Upsert.ToString(), 
                key.UrlPart, 
                key.PropertyNames, 
                response.Data));

            var resultList = new List<CacheSyncCollection>
            {
                new() 
                { 
                    DataObjectType = typeof(EmbedDataObject), 
                    CacheChanges = operations.ToArray() 
                }
            };

            return ActionHandlerOutcome.Successful(output, resultList);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to generate embeddings");
            
            return ActionHandlerOutcome.Failed(new StandardActionFailure
            {
                Code = ex.StatusCode?.ToString() ?? "500",
                Errors = new[]
                {
                    new Error
                    {
                        Source = new[] { "CreateEmbedHandler" },
                        Text = ex.Message
                    }
                }
            });
        }
    }
}
