namespace Connector.App.v1.Vector;

using System.Collections.Generic;
using System.Text.Json;
using Json.Schema.Generation;
using Connector.App.v1.Vector.Create;
using System;


public class VectorDataObject
{
    [Required]
    [Description("The unique identifier for the vector")]
    public string Id { get; set; } = string.Empty;

    [Required]
    [Description("Dense vector values representing the embedding")]
    public float[] Values { get; set; } = Array.Empty<float>();

    [Description("Optional sparse vector representation")]
    public SparseValues? SparseValues { get; set; }

    [Description("Optional metadata associated with the vector")]
    public Dictionary<string, JsonElement>? Metadata { get; set; }

    [Description("Optional namespace for the vector")]
    public string? Namespace { get; set; }
}