using System;
using System.Collections.Generic;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AgentSquad.Runner.Tests;

public class DataValidatorTests
{
    private readonly ILoggerFactory _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

    [Fact]
    public async Task ValidateDataJson_WithValidFile_ReturnsSuccess()
    {
        var dataFilePath = GetDataJsonPath();
        var logger = _loggerFactory.CreateLogger<DataValidator>();
        var validator = new DataValidator(logger);

        var result = await validator.ValidateDataJsonAsync(dataFilePath);

        Assert.True(result.IsValid, result.Message);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task ValidateDataJson_WithMissingFile_ReturnsFalse()
    {
        var logger = _loggerFactory.CreateLogger<DataValidator>();
        var validator = new DataValidator(logger);

        var result = await validator.ValidateDataJsonAsync("/nonexistent/path/data.json");

        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Message);
    }

    [Fact]
    public async Task ValidateDataJson_AllFieldsPopulated()
    {
        var dataFilePath = GetDataJsonPath();
        var logger = _loggerFactory.CreateLogger<DataValidator>();
        var validator = new DataValidator(logger);

        var result = await validator.ValidateDataJsonAsync(dataFilePath);

        Assert.True(result.IsValid);
        var project = result.Data;

        Assert.NotNull(project.Name);
        Assert.NotNull(project.Description);
        Assert.NotNull(project.Milestones);
        Assert.NotNull(project.WorkItems);
        Assert.NotNull(project.Metrics);
    }

    private string GetDataJsonPath()
    {
        var testProjectDir = AppContext.BaseDirectory;
        var solutionDir = Path.GetFullPath(Path.Combine(testProjectDir, "..", "..", "..", ".."));
        return Path.Combine(solutionDir, "src", "AgentSquad.Runner", "data.json");
    }
}