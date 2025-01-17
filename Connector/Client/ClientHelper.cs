using Microsoft.Extensions.DependencyInjection;
using System;
using Xchange.Connector.SDK.Client.ConnectivityApi.Models;
using ESR.Hosting.Client;
using System.Text.Json;
using Xchange.Connector.SDK.Client.AuthTypes;
using Connector.Connections;
using System.Net.Http;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

namespace Connector.Client
{
    /// <summary>
    /// Helper class for configuring client services and dependencies.
    /// </summary>
    public static class ClientHelper
    {
        /// <summary>
        /// Authentication type key constants.
        /// </summary>
        public static class AuthTypeKeyEnums
        {
            public const string ApiKeyAuth = "apiKeyAuth";
        }

        /// <summary>
        /// Configures services based on the active connection configuration.
        /// </summary>
        /// <param name="serviceCollection">The service collection to configure.</param>
        /// <param name="activeConnection">The active connection configuration.</param>
        public static void ResolveServices(this IServiceCollection serviceCollection, ConnectionContainer activeConnection)
        {
            if (activeConnection == null)
            {
                throw new ArgumentNullException(nameof(activeConnection));
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() },
                PropertyNameCaseInsensitive = true
            };

            switch (activeConnection.DefinitionKey)
            {
                case AuthTypeKeyEnums.ApiKeyAuth:
                    ConfigureApiKeyAuth(serviceCollection, activeConnection, options);
                    break;

                default:
                    throw new ArgumentException($"Unsupported authentication type: {activeConnection.DefinitionKey}");
            }
        }

        private static void ConfigureApiKeyAuth(
            IServiceCollection serviceCollection, 
            ConnectionContainer activeConnection, 
            JsonSerializerOptions options)
        {
            var configApiKeyAuth = JsonSerializer.Deserialize<ApiKeyAuth>(activeConnection.Configuration, options)
                ?? throw new InvalidOperationException("Failed to deserialize API key configuration");

            // Register auth configuration
            serviceCollection.AddSingleton<IApiKeyAuth>(configApiKeyAuth);

            // Register HTTP message handlers
            serviceCollection.AddTransient<ApiKeyAuthHandler>();
            serviceCollection.AddTransient<RetryPolicyHandler>();

            // Configure HTTP client with retry policy and auth handler
            serviceCollection.AddHttpClient<ApiClient>(client =>
            {
                client.BaseAddress = new Uri(configApiKeyAuth.BaseUrl);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddPolicyHandler(GetRetryPolicy())
            .AddHttpMessageHandler<ApiKeyAuthHandler>()
            .AddHttpMessageHandler<RetryPolicyHandler>();

            // Register connection test handler
            serviceCollection.AddScoped<ConnectionTestHandler>();
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(3, retryAttempt => 
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
    }
}