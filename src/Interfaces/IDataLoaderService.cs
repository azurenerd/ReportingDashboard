using System.Threading.Tasks;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Interfaces
{
    public interface IDataLoaderService
    {
        /// <summary>
        /// Load and deserialize data.json file.
        /// </summary>
        /// <param name="dataPath">Optional file path; defaults to appsettings:AppSettings:DataPath</param>
        /// <returns>Deserialized ProjectReport object</returns>
        /// <exception cref="System.IO.FileNotFoundException">If data.json not found at path</exception>
        /// <exception cref="System.Text.Json.JsonException">If JSON is malformed or invalid</exception>
        /// <exception cref="System.IO.IOException">If file cannot be read</exception>
        Task<ProjectReport> LoadAsync(string dataPath = null);

        /// <summary>
        /// Get configured data path from appsettings.json.
        /// </summary>
        /// <returns>Data file path (relative or absolute)</returns>
        string GetConfiguredDataPath();
    }
}