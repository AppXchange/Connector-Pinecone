namespace Connector.App.v1;

using Connector.App.v1.Embed;
using Connector.App.v1.Index;
using Connector.App.v1.Vector;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.Json;
using Xchange.Connector.SDK.Abstraction.Hosting;
using Xchange.Connector.SDK.CacheWriter;
using Xchange.Connector.SDK.Abstraction.Change;
using ESR.Hosting.CacheWriter;
using Xchange.Connector.SDK.Hosting.Configuration;
using System.IO;
using Connector.Client;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Connector.App.v1.Common;
using System.Dynamic;

/// <summary>
/// Defines the service configuration for the cache writer component.
/// </summary>
public class AppV1CacheWriterServiceDefinition : BaseCacheWriterServiceDefinition<AppV1CacheWriterConfig>
{
    /// <summary>
    /// Gets the module identifier for the cache writer.
    /// </summary>
    public override string ModuleId => "app-1";

    /// <summary>
    /// Gets the service type for the cache writer.
    /// </summary>
    public override Type ServiceType => typeof(GenericCacheWriterService<AppV1CacheWriterConfig>);

    /// <summary>
    /// Configures the service dependencies for the cache writer.
    /// </summary>
    public override void ConfigureServiceDependencies(IServiceCollection serviceCollection, string serviceConfigJson)
    {
        var serviceConfig = JsonSerializer.Deserialize<AppV1CacheWriterConfig>(serviceConfigJson)
            ?? throw new InvalidOperationException("Failed to deserialize configuration");

        // Register configurations
        serviceCollection.AddSingleton(serviceConfig);
        serviceCollection.AddSingleton<GenericCacheWriterService<AppV1CacheWriterConfig>>();
        serviceCollection.AddSingleton<ICacheWriterServiceDefinition<AppV1CacheWriterConfig>>(this);

        // Register ApiClient and its dependencies
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        // Get the active connection from test settings
        var testSettings = JsonSerializer.Deserialize<TestSettings>(File.ReadAllText("test-settings.json"), options);
        var activeConnection = testSettings?.Settings.Connections
            .FirstOrDefault(c => c.DefinitionKey == testSettings.Settings.ActiveConnection);

        if (activeConnection != null)
        {
            var apiKey = activeConnection.Configuration.GetProperty("apiKey").GetString();
            if (!string.IsNullOrEmpty(apiKey))
            {
                // Register HttpClient with ApiClient
                serviceCollection.AddHttpClient<ApiClient>(client =>
                {
                    client.BaseAddress = new Uri("https://api.pinecone.io");
                    client.DefaultRequestHeaders.Add("Api-Key", apiKey);
                });
            }
        }
        
        // Register data readers
        serviceCollection.AddScoped<VectorDataReader>();
        serviceCollection.AddScoped<IndexDataReader>();
        serviceCollection.AddScoped<EmbedDataReader>();
    }

    /// <summary>
    /// Configures the cache writer service.
    /// </summary>
    public override void ConfigureService(ICacheWriterService service, AppV1CacheWriterConfig config)
    {
        if (!config.Vector.UploadObject && !config.Index.UploadObject && !config.Embed.UploadObject) return;

        if (config.Vector.UploadObject)
        {
            service.RegisterDataReader<VectorDataReader, VectorDataObject>(
                ModuleId,
                "vector",
                config.Vector,
                new DataReaderSettings { UseChangeDetection = true });
        }

        if (config.Index.UploadObject)
        {
            service.RegisterDataReader<IndexDataReader, IndexDataObject>(
                ModuleId,
                "index",
                config.Index,
                new DataReaderSettings { UseChangeDetection = true });
        }

        if (config.Embed.UploadObject)
        {
            service.RegisterDataReader<EmbedDataReader, EmbedDataObject>(
                ModuleId,
                "embed",
                config.Embed,
                new DataReaderSettings { UseChangeDetection = true });
        }
    }

    /// <summary>
    /// Configures the change detector provider.
    /// </summary>
    public override IDataObjectChangeDetectorProvider ConfigureChangeDetectorProvider(
        IChangeDetectorFactory factory,
        ConnectorDefinition connectorDefinition)
    {
        var options = factory.CreateProviderOptionsWithNoDefaultResolver();

        // Create resolvers
        var vectorResolver = new DataObjectKeyResolver<VectorDataObject>();
        var indexResolver = new DataObjectKeyResolver<IndexDataObject>();
        var embedResolver = new DataObjectKeyResolver<EmbedDataObject>();

        try
        {
            // Register resolvers for specific paths
            options.ResolversForDataPath["pinecone/app/1/vector"] = 
                obj => obj is VectorDataObject v ? (v.Id.ToString(), new[] { "Id" }) : (string.Empty, []);
            options.ResolversForDataPath["pinecone/app/1/index"] = 
                obj => obj is IndexDataObject i ? (i.Id.ToString(), new[] { "Id" }) : (string.Empty, []);
            options.ResolversForDataPath["pinecone/app/1/embed"] = 
                obj => obj is EmbedDataObject e ? (e.Id.ToString(), new[] { "Id" }) : (string.Empty, []);
        }
        catch (NotImplementedException)
        {
            // Fallback to default resolver for test environment
            options.ResolveKeyForObject(obj =>
            {
                return obj switch
                {
                    VectorDataObject v => (v.Id.ToString(), new[] { "Id" }),
                    IndexDataObject i => (i.Id.ToString(), new[] { "Id" }),
                    EmbedDataObject e => (e.Id.ToString(), new[] { "Id" }),
                    _ => (string.Empty, Array.Empty<string>())
                };
            });
        }

        return factory.CreateProvider(options);
    }
}

public class TestSettings
{
    public TestSettingsData Settings { get; set; } = new();
}

public class TestSettingsData
{
    public string ActiveConnection { get; set; } = string.Empty;
    public List<ConnectionConfig> Connections { get; set; } = new();
}

public class ConnectionConfig
{
    public string DefinitionKey { get; set; } = string.Empty;
    public JsonElement Configuration { get; set; }
}