using Connector.Client;
using ESR.Hosting.Action;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System;
using Xchange.Connector.SDK.Action;
using Xchange.Connector.SDK.CacheWriter;
using System.Text.Json;
using Xchange.Connector.SDK.Client.AppNetwork;
using ESR.Hosting.CacheWriter;
using System.Linq;

namespace Connector.App.v1.Index.Create;

public class CreateIndexHandler : IActionHandler<CreateIndexAction>
{
    private readonly ILogger<CreateIndexHandler> _logger;
    private readonly ApiClient _apiClient;

    public CreateIndexHandler(
        ILogger<CreateIndexHandler> logger,
        ApiClient apiClient)
    {
        _logger = logger;
        _apiClient = apiClient;
    }
    
    public async Task<ActionHandlerOutcome> HandleQueuedActionAsync(
        ActionInstance actionInstance, 
        CancellationToken cancellationToken)
    {
        var input = JsonSerializer.Deserialize<CreateIndexActionInput>(actionInstance.InputJson, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        if (input == null)
        {
            throw new ArgumentException("Failed to deserialize action input", nameof(actionInstance));
        }
        
        try
        {
            _logger.LogInformation("Checking for existing Pinecone index: {Name}", input.Name);

            // Check if the index already exists
            var existingIndexesResponse = await _apiClient.GetIndexes(cancellationToken);
            if (existingIndexesResponse.IsSuccessful && existingIndexesResponse.Data != null)
            {
                var existingIndex = existingIndexesResponse.Data.FirstOrDefault(i => i.Name.Equals(input.Name, StringComparison.OrdinalIgnoreCase));
                if (existingIndex != null)
                {
                    return ActionHandlerOutcome.Failed(new StandardActionFailure
                    {
                        Code = "409", // Conflict
                        Errors = new[]
                        {
                            new Error
                            {
                                Source = new[] { "CreateIndexHandler" },
                                Text = $"An index with the name '{input.Name}' already exists."
                            }
                        }
                    });
                }
            }

            _logger.LogInformation("Creating Pinecone index: {Name}", input.Name);

            var indexObject = new IndexDataObject
            {
                Id = input.Name,
                Name = input.Name,
                Dimension = input.Dimension,
                Metric = input.Metric,
                Spec = input.Spec ?? new IndexSpec()
            };

            var response = await _apiClient.CreateIndex(indexObject, cancellationToken);

            if (!response.IsSuccessful)
            {
                return ActionHandlerOutcome.Failed(new StandardActionFailure
                {
                    Code = response.StatusCode.ToString(),
                    Errors = new[]
                    {
                        new Error
                        {
                            Source = new[] { "CreateIndexHandler" },
                            Text = response.ErrorMessage ?? "Failed to create index"
                        }
                    }
                });
            }

            var output = new CreateIndexActionOutput
            {
                Name = response.Data!.Name,
                Status = response.Data.Status ?? new IndexStatus(),
                Host = response.Data.Host ?? string.Empty
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
                    DataObjectType = typeof(IndexDataObject), 
                    CacheChanges = operations.ToArray() 
                }
            };

            return ActionHandlerOutcome.Successful(output, resultList);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to create index {Name}", input.Name);
            
            return ActionHandlerOutcome.Failed(new StandardActionFailure
            {
                Code = ex.StatusCode?.ToString() ?? "500",
                Errors = new[]
                {
                    new Error
                    {
                        Source = new[] { "CreateIndexHandler" },
                        Text = ex.Message
                    }
                }
            });
        }
    }
}
