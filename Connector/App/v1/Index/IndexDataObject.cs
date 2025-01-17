namespace Connector.App.v1.Index;

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
[Description("Configuration for creating a Pinecone index.")]
public class IndexDataObject
{
    [JsonPropertyName("id")]
    [Description("Example primary key of the object")]
    public required string Id { get; set; }

    [JsonPropertyName("name")]
    [Description("The name of the index. Resource name must be 1-45 characters long, start and end with an alphanumeric character, and consist only of lower case alphanumeric characters or '-'.")]
    [MinLength(1)]
    [MaxLength(45)]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("dimension")]
    [Description("The dimensions of the vectors to be inserted in the index (1 < dimension < 20000).")]
    [Minimum(2)]
    [Maximum(19999)]
    public int Dimension { get; set; }

    [JsonPropertyName("metric")]
    [Description("The distance metric to be used for similarity search.")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MetricType Metric { get; set; } = MetricType.Cosine;

    [JsonPropertyName("spec")]
    [Description("Defines how the index should be deployed.")]
    public IndexSpec? Spec { get; set; }

    [JsonPropertyName("deletion_protection")]
    [Description("Whether deletion protection is enabled/disabled for the index.")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DeletionProtectionType DeletionProtection { get; set; } = DeletionProtectionType.Disabled;

    [JsonPropertyName("host")]
    [Description("The URL address where the index is hosted.")]
    public string? Host { get; set; }

    [JsonPropertyName("status")]
    [Description("The status of the index.")]
    public IndexStatus? Status { get; set; }

    [JsonPropertyName("tags")]
    [Description("Custom user tags added to an index. Keys must be 80 characters or less. Values must be 120 characters or less.")]
    public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
}

public class IndexSpec
{
    [JsonPropertyName("serverless")]
    [Description("Configuration needed to deploy a serverless index.")]
    public ServerlessSpec? Serverless { get; set; }

    [JsonPropertyName("pod")]
    [Description("Configuration needed to deploy a pod-based index.")]
    public object? Pod { get; set; }
}

public class ServerlessSpec
{
    [JsonPropertyName("cloud")]
    [Description("The cloud provider for the serverless index.")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CloudProvider Cloud { get; set; }

    [JsonPropertyName("region")]
    [Description("The region where the serverless index will be deployed.")]
    public string Region { get; set; } = string.Empty;
}

public class PodSpec
{
    [JsonPropertyName("environment")]
    [Description("The environment where the pod-based index will be hosted.")]
    public string Environment { get; set; } = string.Empty;

    [JsonPropertyName("pod_type")]
    [Description("The type of pod to use for the index.")]
    public string PodType { get; set; } = string.Empty;

    [JsonPropertyName("pods")]
    [Description("The number of pods for the index.")]
    public int Pods { get; set; }

    [JsonPropertyName("replicas")]
    [Description("The number of replicas for the index.")]
    public int Replicas { get; set; }

    [JsonPropertyName("shards")]
    [Description("The number of shards for the index.")]
    public int Shards { get; set; }
}

public class IndexStatus
{
    [JsonPropertyName("ready")]
    [Description("Indicates if the index is ready.")]
    public bool Ready { get; set; }

    [JsonPropertyName("state")]
    [Description("The current state of the index.")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public IndexState State { get; set; }
}

public enum MetricType
{
    Cosine,
    Euclidean,
    Dotproduct
}

public enum DeletionProtectionType
{
    Disabled,
    Enabled
}

public enum CloudProvider
{
    aws,
    gcp,
    azure
}

public enum IndexState
{
    Initializing,
    InitializationFailed,
    ScalingUp,
    ScalingDown,
    ScalingUpPodSize,
    ScalingDownPodSize,
    Terminating,
    Ready
}

public class PineconeIndex
{
    public required string Name { get; set; }
    public required int Dimension { get; set; }
    public required string Host { get; set; }
    public required string Metric { get; set; }
    public required object Spec { get; set; }
    public required object Status { get; set; }
    public string? DeletionProtection { get; set; }
    public Dictionary<string, string>? Tags { get; set; }
}