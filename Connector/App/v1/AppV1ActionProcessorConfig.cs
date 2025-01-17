namespace Connector.App.v1;

using Json.Schema.Generation;
using System.ComponentModel.DataAnnotations;
using ESR.Hosting.Action;
using ESR.Hosting;
using Xchange.Connector.SDK.Action;

[Title("App V1 Action Processor Configuration")]
[Description("Configuration of the data object actions for the module.")]
public class AppV1ActionProcessorConfig
{
    [Json.Schema.Generation.Required]
    [Title("Vector Configuration")]
    [Description("Configuration for vector-related operations")]
    public VectorConfig Vector { get; set; } = new();

    [Json.Schema.Generation.Required]
    [Title("Index Configuration")]
    [Description("Configuration for index-related operations")]
    public IndexConfig Index { get; set; } = new();

    [Json.Schema.Generation.Required]
    [Title("Embed Configuration")]
    [Description("Configuration for embedding-related operations")]
    public EmbedConfig Embed { get; set; } = new();

    [Title("Create Vector Handler Configuration")]
    [Description("Configuration for vector creation handler")]
    public DefaultActionHandlerConfig CreateVectorHandlerConfig { get; set; } = new();

    [Title("Create Index Handler Configuration")]
    [Description("Configuration for index creation handler")]
    public DefaultActionHandlerConfig CreateIndexHandlerConfig { get; set; } = new();

    [Title("Create Embed Handler Configuration")]
    [Description("Configuration for embed creation handler")]
    public DefaultActionHandlerConfig CreateEmbedHandlerConfig { get; set; } = new();
}

public class VectorConfig
{
    [Json.Schema.Generation.Required]
    [Description("API Key for accessing the vector service")]
    public string ApiKey { get; set; } = string.Empty;

    [Json.Schema.Generation.Required]
    [Description("Host URL for the vector index")]
    public string IndexHost { get; set; } = string.Empty;
}

public class IndexConfig
{
    [Json.Schema.Generation.Required]
    [Description("API Key for accessing the index service")]
    public string ApiKey { get; set; } = string.Empty;

    [Json.Schema.Generation.Required]
    [Description("Base URL for the index service")]
    public string BaseUrl { get; set; } = string.Empty;
}

public class EmbedConfig
{
    [Json.Schema.Generation.Required]
    [Description("API Key for accessing the embedding service")]
    public string ApiKey { get; set; } = string.Empty;

    [Json.Schema.Generation.Required]
    [Description("Base URL for the embedding service")]
    public string BaseUrl { get; set; } = string.Empty;

    [Json.Schema.Generation.Required]
    [Description("Default model to use for embedding generation")]
    public string DefaultModel { get; set; } = "multilingual-e5-large";

    [Range(1, 2048)]
    [Description("Maximum batch size for embedding requests")]
    public int MaxBatchSize { get; set; } = 100;
}