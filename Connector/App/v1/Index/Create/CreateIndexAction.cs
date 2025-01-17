namespace Connector.App.v1.Index.Create;

using Json.Schema.Generation;
using System.Text.Json.Serialization;
using Xchange.Connector.SDK.Action;

/// <summary>
/// Action for creating a new Pinecone index.
/// </summary>
[Description("Creates a new index in the Pinecone vector database")]
public class CreateIndexAction : IStandardAction<CreateIndexActionInput, CreateIndexActionOutput>
{
    public CreateIndexActionInput ActionInput { get; set; } = new();
    public CreateIndexActionOutput ActionOutput { get; set; } = new();
    public StandardActionFailure ActionFailure { get; set; } = new();

    public bool CreateRtap => true;
}

/// <summary>
/// Input parameters for creating a Pinecone index.
/// </summary>
public class CreateIndexActionInput
{
    [JsonPropertyName("name")]
    [Description("The name of the index")]
    [Required]
    [MinLength(1)]
    [MaxLength(45)]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("dimension")]
    [Description("The dimensions of the vectors to be inserted in the index")]
    [Minimum(2)]
    [Maximum(19999)]
    public int Dimension { get; set; }

    [JsonPropertyName("metric")]
    [Description("The distance metric to be used for similarity search")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MetricType Metric { get; set; } = MetricType.Cosine;

    [JsonPropertyName("spec")]
    [Description("Defines how the index should be deployed")]
    public IndexSpec? Spec { get; set; }
}

/// <summary>
/// Output data for the created Pinecone index.
/// </summary>
public class CreateIndexActionOutput
{
    [JsonPropertyName("name")]
    [Description("The name of the created index")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    [Description("The status of the created index")]
    public IndexStatus Status { get; set; } = new();

    [JsonPropertyName("host")]
    [Description("The host URL where the index is deployed")]
    public string Host { get; set; } = string.Empty;
}
