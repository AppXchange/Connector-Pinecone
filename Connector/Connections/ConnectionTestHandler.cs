using Connector.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Xchange.Connector.SDK.Client.Testing;
using System.Net;

namespace Connector.Connections
{
    public class ConnectionTestHandler : IConnectionTestHandler
    {
        private readonly ILogger<IConnectionTestHandler> _logger;
        private readonly ApiClient _apiClient;

        public ConnectionTestHandler(ILogger<IConnectionTestHandler> logger, ApiClient apiClient)
        {
            _logger = logger;
            _apiClient = apiClient;
        }

        public async Task<TestConnectionResult> TestConnection()
        {
            // Make a call to the /indexes endpoint to test the connection
            var response = await _apiClient.GetIndexes(); // Assuming GetIndexes() is implemented in ApiClient

            // Depending on the response, make your own specific messages.
            if (response == null)
            {
                return new TestConnectionResult()
                {
                    Success = false,
                    Message = "Failed to get response from server",
                    StatusCode = 500
                };
            }

            if (response.IsSuccessful)
            {
                return new TestConnectionResult()
                {
                    Success = true,
                    Message = "Successful test. Indexes retrieved.",
                    StatusCode = (int)response.StatusCode
                };
            }

            switch (response.StatusCode)
            {
                case HttpStatusCode.Forbidden:
                    return new TestConnectionResult()
                    {
                        Success = false,
                        Message = "Invalid Credentials: Forbidden.",
                        StatusCode = (int)response.StatusCode
                    };
                case HttpStatusCode.Unauthorized:
                    return new TestConnectionResult()
                    {
                        Success = false,
                        Message = "Invalid Credentials: Unauthorized",
                        StatusCode = (int)response.StatusCode
                    };
                default:
                    return new TestConnectionResult()
                    {
                        Success = false,
                        Message = "Unknown Issue.",
                        StatusCode = (int)response.StatusCode
                    };
            }
        }
    }
}
