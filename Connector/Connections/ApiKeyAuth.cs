using Xchange.Connector.SDK.Client.AuthTypes;
using Xchange.Connector.SDK.Client.ConnectionDefinitions.Attributes;

namespace Connector.Connections;

[ConnectionDefinition(title: "ApiKeyAuth", description: "")]
public class ApiKeyAuth : IApiKeyAuth
{
    [ConnectionProperty(title: "ApiKey", description: "", isRequired: true, isSensitive: true)]
    public string ApiKey { get; init; } = string.Empty;

    [ConnectionProperty(title: "Connection Environment", description: "", isRequired: true, isSensitive: false)]
    public ConnectionEnvironmentApiKeyAuth ConnectionEnvironment { get; set; } = ConnectionEnvironmentApiKeyAuth.Unknown;

    public string BaseUrl => "https://api.pinecone.io";
}

public enum ConnectionEnvironmentApiKeyAuth
{
    Unknown = 0,
    Production = 1,
    Test = 2
}