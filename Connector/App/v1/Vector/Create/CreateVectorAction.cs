namespace Connector.App.v1.Vector.Create;

using Json.Schema.Generation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xchange.Connector.SDK.Action;

/// <summary>
/// Action for creating or upserting vectors in the Pinecone database.
/// </summary>
[Description("Creates or updates vectors in the Pinecone vector database")]
public class CreateVectorAction : IStandardAction<CreateVectorActionInput, CreateVectorActionOutput>
{
    public CreateVectorActionInput ActionInput { get; set; } = new();
    public CreateVectorActionOutput ActionOutput { get; set; } = new();
    public StandardActionFailure ActionFailure { get; set; } = new();

    public bool CreateRtap => true;
}

/// <summary>
/// Input parameters for vector creation/upsert.
/// </summary>
public class CreateVectorActionInput
{
    [JsonPropertyName("id")]
    [Description("Unique identifier for the vector")]
    [System.ComponentModel.DataAnnotations.Required]
    [StringLength(512, MinimumLength = 1)]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("values")]
    [Description("Dense vector values representing the embedding")]
    [System.ComponentModel.DataAnnotations.Required]
    public float[] Values { get; set; } = Array.Empty<float>();

    [JsonPropertyName("sparseValues")]
    [Description("Optional sparse vector representation")]
    public SparseValues? SparseValues { get; set; }

    [JsonPropertyName("metadata")]
    [Description("Optional metadata associated with the vector")]
    public Dictionary<string, JsonElement>? Metadata { get; set; }

    [JsonPropertyName("namespace")]
    [Description("Optional namespace for the vector")]
    public string? Namespace { get; set; }
}

/// <summary>
/// Output data for the created/upserted vector.
/// </summary>
public class CreateVectorActionOutput
{
    [JsonPropertyName("id")]
    [Description("The ID of the created/updated vector")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("namespace")]
    [Description("The namespace where the vector was stored")]
    public string? Namespace { get; set; }
}

/// <summary>
/// Represents sparse vector data with indices and corresponding values.
/// </summary>
public class SparseValues
{
    [JsonPropertyName("indices")]
    [Description("Indices for the sparse vector values")]
    [System.ComponentModel.DataAnnotations.Required]
    public int[] Indices { get; set; } = Array.Empty<int>();

    [JsonPropertyName("values")]
    [Description("Values corresponding to the sparse vector indices")]
    [System.ComponentModel.DataAnnotations.Required]
    public float[] Values { get; set; } = Array.Empty<float>();
}
