namespace AgentSquad.Runner.Interfaces;

using AgentSquad.Runner.Models;

/// <summary>
/// Service contract for loading and deserializing JSON data files into ProjectReport POCOs.
/// 
/// Responsibility: Load, deserialize, and parse JSON file into ProjectReport POCO.
/// Single responsibility: JSON I/O and deserialization. No file watching, no validation.
/// 
/// Lifetime: Scoped (per HTTP request in Blazor Server context)
/// </summary>
public interface IDataLoaderService
{
    /// <summary>
    /// Load and deserialize data.json file into a ProjectReport object.
    /// </summary>
    /// <param name="dataPath">
    /// Optional file path to load from. If null, defaults to the configured DataPath from appsettings.json.
    /// Can be relative (e.g., "data.json") or absolute (e.g., "C:\data\project.json").
    /// </param>
    /// <returns>
    /// Task that completes with a deserialized ProjectReport object.
    /// </returns>
    /// <exception cref="FileNotFoundException">
    /// Thrown if the data file does not exist at the specified path.
    /// Message: "Data file not found at path: {dataPath}"
    /// </exception>
    /// <exception cref="System.Text.Json.JsonException">
    /// Thrown if JSON is malformed or cannot be deserialized to ProjectReport.
    /// Message includes details about the JSON parse error.
    /// </exception>
    /// <exception cref="IOException">
    /// Thrown if file cannot be read (access denied, file in use, etc.).
    /// Message describes the I/O error.
    /// </exception>
    /// <remarks>
    /// - Resolves dataPath from parameter, config, or default "./data.json"
    /// - Checks file exists; throws FileNotFoundException if not
    /// - Reads file content asynchronously via System.IO.File.ReadAllTextAsync()
    /// - Deserializes via System.Text.Json.JsonSerializer with PropertyNameCaseInsensitive option
    /// - Logs all operations at INFO level
    /// - Does NOT validate schema (DataValidator handles that)
    /// </remarks>
    Task<ProjectReport> LoadAsync(string dataPath = null);

    /// <summary>
    /// Get the configured data file path from appsettings.json configuration.
    /// </summary>
    /// <returns>
    /// The data file path configured via AppSettings:DataPath in appsettings.json.
    /// Defaults to "data.json" if not configured.
    /// Can be relative or absolute path.
    /// </returns>
    /// <remarks>
    /// Returns the value from IConfiguration["AppSettings:DataPath"].
    /// Does not check if file exists; that is caller's responsibility.
    /// </remarks>
    string GetConfiguredDataPath();
}