namespace Connector.App.v1;

using Connector.Client;
using Connector.App.v1.Embed.Create;
using Connector.App.v1.Index.Create;
using Connector.App.v1.Vector.Create;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.Json;
using Xchange.Connector.SDK.Abstraction.Hosting;
using Xchange.Connector.SDK.Action;
using ESR.Hosting;

/// <summary>
/// Defines the service configuration for the Pinecone connector's action processor.
/// </summary>
public class AppV1ActionProcessorServiceDefinition : BaseActionHandlerServiceDefinition<AppV1ActionProcessorConfig>
{
    /// <summary>
    /// Gets the module identifier for the action processor.
    /// </summary>
    public override string ModuleId => "app-1";

    /// <summary>
    /// Gets the service type for the action processor.
    /// </summary>
    public override Type ServiceType => typeof(GenericActionHandlerService<AppV1ActionProcessorConfig>);

    /// <summary>
    /// Configures the service dependencies for the action processor.
    /// </summary>
    public override void ConfigureServiceDependencies(IServiceCollection serviceCollection, string serviceConfigJson)
    {
        try
        {
            // Register minimum required services for metadata extraction
            serviceCollection.AddSingleton<GenericActionHandlerService<AppV1ActionProcessorConfig>>();
            serviceCollection.AddSingleton<IActionHandlerServiceDefinition<AppV1ActionProcessorConfig>>(this);
            
            // Add test data source registration
            serviceCollection.AddSingleton<Xchange.Connector.SDK.Test.Local.ITestDataSource, Xchange.Connector.SDK.Test.Local.TestDataSource>();
            
            // Register handlers as transient
            serviceCollection.AddTransient<CreateVectorHandler>();
            serviceCollection.AddTransient<CreateIndexHandler>();
            serviceCollection.AddTransient<CreateEmbedHandler>();

            // Only configure additional services if we have config
            if (!string.IsNullOrEmpty(serviceConfigJson))
            {
                var config = JsonSerializer.Deserialize<AppV1ActionProcessorConfig>(
                    serviceConfigJson, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (config != null)
                {
                    serviceCollection.AddSingleton(config);
                    serviceCollection.AddLogging();
                    serviceCollection.AddSingleton<IServiceCollection>(serviceCollection);
                    serviceCollection.AddSingleton<IHostContext, HostContext>();
                    serviceCollection.AddScoped<Subscriber>();
                    serviceCollection.AddSingleton<ServiceConfig>();
                    serviceCollection.AddSingleton<SystemConfig>();
                    serviceCollection.AddSingleton<System>();
                    serviceCollection.AddSingleton<Service>();
                    serviceCollection.AddSingleton<ServiceRunRequest>();
                    serviceCollection.AddHttpClient();
                    serviceCollection.AddScoped<ApiClient>();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ConfigureServiceDependencies: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Configures the action handlers and their actions.
    /// </summary>
    public override void ConfigureService(IActionHandlerService service, AppV1ActionProcessorConfig config)
    {
        try
        {
            if (service == null) return;

            // Handle both GenericActionHandlerService and ExtractorActionHandlerService
            if (service.GetType().Name == "ExtractorActionHandlerService")
            {
                // For metadata extraction, just register the actions without validation
                var defaultConfig = new DefaultActionHandlerConfig { ProcessQueuedEvent = true };

                service.RegisterHandlerForDataObjectAction<CreateVectorHandler, CreateVectorAction>(
                    ModuleId, "vector", "create", defaultConfig);
                service.RegisterHandlerForDataObjectAction<CreateIndexHandler, CreateIndexAction>(
                    ModuleId, "index", "create", defaultConfig);
                service.RegisterHandlerForDataObjectAction<CreateEmbedHandler, CreateEmbedAction>(
                    ModuleId, "embed", "create", defaultConfig);
                return;
            }

            // Normal runtime configuration
            if (service is not GenericActionHandlerService<AppV1ActionProcessorConfig> actionHandlerService)
            {
                throw new ArgumentException(
                    $"Expected service of type {typeof(GenericActionHandlerService<AppV1ActionProcessorConfig>)}, but got {service.GetType().FullName}", 
                    nameof(service));
            }

            var runtimeConfig = new DefaultActionHandlerConfig 
            { 
                ProcessQueuedEvent = true 
            };

            actionHandlerService.RegisterHandlerForDataObjectAction<CreateVectorHandler, CreateVectorAction>(
                ModuleId, "vector", "create", runtimeConfig);
            actionHandlerService.RegisterHandlerForDataObjectAction<CreateIndexHandler, CreateIndexAction>(
                ModuleId, "index", "create", runtimeConfig);
            actionHandlerService.RegisterHandlerForDataObjectAction<CreateEmbedHandler, CreateEmbedAction>(
                ModuleId, "embed", "create", runtimeConfig);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error configuring service: {ex.Message}");
            throw;
        }
    }
}