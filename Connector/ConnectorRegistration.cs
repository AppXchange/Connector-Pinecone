namespace Connector;
using Connector.App.v1;
using Connector.Client;
using Connector.Connections;
using ESR.Hosting;
using ESR.Hosting.Client;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Xchange.Connector.SDK.Abstraction.Hosting;
using Xchange.Connector.SDK.Client.Testing;
using Xchange.Connector.SDK.Client.AuthTypes;
using System;
using Microsoft.Extensions.Logging;

/// <summary>
/// The registration object for the Pinecone connector.
/// </summary>
public class ConnectorRegistration : 
    IConnectorRegistration<ConnectorRegistrationConfig>, 
    IConfigureConnectorApiClient
{
    /// <summary>
    /// Configures services required by the connector.
    /// </summary>
    public void ConfigureServices(IServiceCollection serviceCollection, IHostContext hostContext)
    {
        if (hostContext == null) throw new ArgumentNullException(nameof(hostContext));

        try
        {
            var systemConfig = hostContext.GetSystemConfig()
                ?? throw new InvalidOperationException("System configuration is missing");

            var connectorRegistrationConfig = JsonSerializer.Deserialize<ConnectorRegistrationConfig>(
                systemConfig.Configuration,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new InvalidOperationException("Failed to deserialize connector configuration");

            // Register configurations
            serviceCollection.AddSingleton(connectorRegistrationConfig);

            // Register HTTP handlers
            serviceCollection.AddTransient<RetryPolicyHandler>();
            serviceCollection.AddTransient<ApiKeyAuthHandler>();

            // Register ApiClient with proper configuration
            serviceCollection.AddHttpClient<ApiClient>()
                .AddHttpMessageHandler<ApiKeyAuthHandler>()
                .AddHttpMessageHandler<RetryPolicyHandler>();

            serviceCollection.AddScoped<ApiClient>();

            // Register auth configuration
            serviceCollection.AddSingleton<IApiKeyAuth>(connectorRegistrationConfig.Auth.ApiKey);

            // Add logging
            serviceCollection.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ConfigureServices: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    /// <summary>
    /// Registers service definitions for action processing and cache writing.
    /// </summary>
    public void RegisterServiceDefinitions(IServiceCollection serviceCollection)
    {
        try
        {
            serviceCollection.AddSingleton<IConnectorServiceDefinition, AppV1ActionProcessorServiceDefinition>();
            serviceCollection.AddSingleton<IConnectorServiceDefinition, AppV1CacheWriterServiceDefinition>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in RegisterServiceDefinitions: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    /// <summary>
    /// Configures the API client with connection settings.
    /// </summary>
    public void ConfigureConnectorApiClient(
        IServiceCollection serviceCollection, 
        IHostConnectionContext hostConnectionContext)
    {
        if (hostConnectionContext == null) throw new ArgumentNullException(nameof(hostConnectionContext));

        try
        {
            var activeConnection = hostConnectionContext.GetConnection()
                ?? throw new InvalidOperationException("No active connection found");

            // Configure services based on the active connection
            serviceCollection.ResolveServices(activeConnection);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ConfigureConnectorApiClient: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    /// <summary>
    /// Registers the connection test handler.
    /// </summary>
    public void RegisterConnectionTestHandler(IServiceCollection serviceCollection)
    {
        try
        {
            serviceCollection.AddScoped<IConnectionTestHandler, ConnectionTestHandler>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in RegisterConnectionTestHandler: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }
}