namespace Connector.App.v1;

using Json.Schema.Generation;
using ESR.Hosting.CacheWriter;

[Title("App V1 Cache Writer Configuration")]
[Description("Configuration of the data object caches for the module.")]
public class AppV1CacheWriterConfig
{
    [Title("Vector Configuration")]
    [Description("Configuration for vector data caching")]
    public CacheWriterObjectConfig Vector { get; set; } = new();

    [Title("Index Configuration")]
    [Description("Configuration for index data caching")]
    public CacheWriterObjectConfig Index { get; set; } = new();

    [Title("Embed Configuration")]
    [Description("Configuration for embed data caching")]
    public CacheWriterObjectConfig Embed { get; set; } = new();
}