namespace Connector.App.v1.Embed;

using Json.Schema.Generation;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Xchange.Connector.SDK.CacheWriter;

/// <summary>
/// Data object that will represent an object in the Xchange system. This will be converted to a JsonSchema, 
/// so add attributes to the properties to provide any descriptions, titles, ranges, max, min, etc... 
/// These types will be used for validation at runtime to make sure the objects being passed through the system 
/// are properly formed. The schema also helps provide integrators more information for what the values 
/// are intended to be.
/// </summary>
[PrimaryKey("id", nameof(Id))]
[Description("Example description of the object.")]
public class EmbedDataObject
{
    [JsonPropertyName("id")]
    [Description("Example primary key of the object")]
    public required string Id { get; set; }

    [JsonPropertyName("inputs")]
    [Description("List of inputs to generate embeddings for.")]
    public List<EmbedInput> Inputs { get; set; } = new();

    [JsonPropertyName("model")]
    [Description("The model to use for embedding generation.")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("parameters")]
    [Description("Model-specific parameters.")]
    public Parameters Parameters { get; set; } = new Parameters();
}

public class Parameters
{
    [JsonPropertyName("input_type")]
    [Description("Common property used to distinguish between types of data.")]
    public string InputType { get; set; } = string.Empty;

    [JsonPropertyName("truncate")]
    [Description("How to handle inputs longer than those supported by the model.")]
    public string Truncate { get; set; } = "END";
}

public class EmbedInput
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}