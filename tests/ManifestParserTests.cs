using System.Text.Json;
using Amaspire.Models;
using Xunit;

namespace Amaspire.Tests;

public class ManifestParserTests
{
    [Fact]
    public void ParsesBasicProjectResource()
    {
        var json = """
        {
          "resources": {
            "apiservice": {
              "type": "project.v0",
              "image": "myapp-api",
              "env": {
                "KEY": "value"
              },
              "bindings": {
                "http": {
                  "scheme": "http",
                  "containerPort": 8080
                }
              }
            }
          }
        }
        """;

        var manifest = JsonSerializer.Deserialize<AspireManifest>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(manifest);
        Assert.Single(manifest.Resources);
        Assert.Equal("project.v0", manifest.Resources["apiservice"].Type);
        Assert.Equal("myapp-api", manifest.Resources["apiservice"].Image);
        Assert.Equal(8080, manifest.Resources["apiservice"].Bindings?["http"].ContainerPort);
    }
}
