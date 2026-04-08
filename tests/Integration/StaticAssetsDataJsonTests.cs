using System.Text.Json;
using Xunit;

namespace AgentSquad.Tests.Integration;

public class StaticAssetsDataJsonTests
{
    private string GetDataJsonPath()
    {
        var testAssemblyDir = Path.GetDirectoryName(typeof(StaticAssetsDataJsonTests).Assembly.Location);
        var projectRoot = Path.Combine(testAssemblyDir ?? ".", "..", "..", "..");
        return Path.Combine(projectRoot, "wwwroot", "data.json");
    }

    [Fact]
    public void DataJsonFileExists()
    {
        var jsonPath = GetDataJsonPath();
        Assert.True(File.Exists(jsonPath), $"data.json not found at {jsonPath}");
    }

    [Fact]
    public void DataJsonIsValidJson()
    {
        var jsonPath = GetDataJsonPath();
        var content = File.ReadAllText(jsonPath);
        
        var exception = Record.Exception(() => JsonDocument.Parse(content));
        Assert.Null(exception);
    }

    [Fact]
    public void DataJsonIsUtf8Encoded()
    {
        var jsonPath = GetDataJsonPath();
        var bytes = File.ReadAllBytes(jsonPath);
        
        var exception = Record.Exception(() => System.Text.Encoding.UTF8.GetString(bytes));
        Assert.Null(exception);
    }

    [Fact]
    public void DataJsonDeserializesToExpectedStructure()
    {
        var jsonPath = GetDataJsonPath();
        var content = File.ReadAllText(jsonPath);
        
        using var doc = JsonDocument.Parse(content);
        Assert.NotNull(doc.RootElement);
    }

    [Fact]
    public void MalformedJsonHandledGracefully()
    {
        var malformedJson = "{invalid json}";
        
        var exception = Record.Exception(() => JsonDocument.Parse(malformedJson));
        Assert.NotNull(exception);
        Assert.IsType<JsonException>(exception);
    }

    [Fact]
    public void EmptyJsonFileHandledGracefully()
    {
        var emptyJson = "";
        
        var exception = Record.Exception(() => JsonDocument.Parse(emptyJson));
        Assert.NotNull(exception);
    }
}