using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MetricType
{
    [JsonPropertyName("cosine")]
    Cosine,
    
    [JsonPropertyName("euclidean")]
    Euclidean,
    
    [JsonPropertyName("dotproduct")]
    DotProduct
} 