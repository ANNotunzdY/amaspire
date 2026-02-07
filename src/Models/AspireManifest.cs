using System.Text.Json.Serialization;

namespace Amaspire.Models;

public class AspireManifest
{
    [JsonPropertyName("resources")]
    public Dictionary<string, Resource> Resources { get; set; } = new();
}

public class Resource
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("env")]
    public Dictionary<string, object>? Env { get; set; }

    [JsonPropertyName("bindings")]
    public Dictionary<string, Binding>? Bindings { get; set; }
}

public class Binding
{
    [JsonPropertyName("scheme")]
    public string? Scheme { get; set; }

    [JsonPropertyName("protocol")]
    public string? Protocol { get; set; }

    [JsonPropertyName("transport")]
    public string? Transport { get; set; }

    [JsonPropertyName("containerPort")]
    public int? ContainerPort { get; set; }
}
