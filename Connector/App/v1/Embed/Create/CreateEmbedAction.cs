namespace Connector.App.v1.Embed.Create;

using Json.Schema.Generation;
using System;
using System.Text.Json.Serialization;
using Xchange.Connector.SDK.Action;

/// <summary>
/// Action for generating embeddings from input text.
/// </summary>
[Description("Generates embeddings for input text using the specified model")]
public class CreateEmbedAction : IStandardAction<CreateEmbedActionInput, CreateEmbedActionOutput>
{
    public CreateEmbedActionInput ActionInput { get; set; } = new();
    public CreateEmbedActionOutput ActionOutput { get; set; } = new();
    public StandardActionFailure ActionFailure { get; set; } = new();

    public bool CreateRtap => true;
}

/// <summary>
/// Input parameters for embedding generation.
/// </summary>
public class CreateEmbedActionInput
{
    [JsonPropertyName("inputs")]
    [Description("List of text inputs to generate embeddings for")]
    [Required]
    [MinLength(1)]
    public EmbedInput[] Inputs { get; set; } = Array.Empty<EmbedInput>();

    [JsonPropertyName("model")]
    [Description("The model to use for embedding generation")]
    public string? Model { get; set; }

    [JsonPropertyName("parameters")]
    [Description("Additional parameters for embedding generation")]
    public Parameters? Parameters { get; set; }
}

public class EmbedInput
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

/// <summary>
/// Output data containing the generated embeddings.
/// </summary>
public class CreateEmbedActionOutput
{
    [JsonPropertyName("embeddings")]
    [Description("The generated embeddings for each input")]
    public float[][] Embeddings { get; set; } = Array.Empty<float[]>();

    [JsonPropertyName("model")]
    [Description("The model used for embedding generation")]
    public string Model { get; set; } = string.Empty;
}
