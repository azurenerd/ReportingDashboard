using System.Text.Json;
using System.Text.Json.Serialization;

namespace ReportingDashboard.Web.Models;

/// <summary>
/// Canonical <see cref="JsonSerializerOptions"/> used for all
/// <c>data.json</c> serialization and deserialization.
/// </summary>
/// <remarks>
/// Behaviors:
/// <list type="bullet">
///   <item><description><see cref="JsonSerializerOptions.PropertyNameCaseInsensitive"/> = true so PM-authored JSON with any casing binds cleanly.</description></item>
///   <item><description><see cref="JsonSerializerOptions.PropertyNamingPolicy"/> = camelCase for symmetric round-tripping with the canonical lowerCamel JSON shape.</description></item>
///   <item><description><see cref="JsonSerializerOptions.ReadCommentHandling"/> = <see cref="JsonCommentHandling.Skip"/> so <c>//</c> and <c>/* */</c> in hand-edited files don't fail parsing.</description></item>
///   <item><description><see cref="JsonSerializerOptions.AllowTrailingCommas"/> = true for the same hand-edit ergonomics.</description></item>
///   <item><description><see cref="JsonStringEnumConverter"/> (camelCase) so enum values appear as <c>"poc"</c>, <c>"inProgress"</c>, etc.</description></item>
/// </list>
/// </remarks>
public static class JsonOptions
{
    /// <summary>Singleton options instance. Safe to reuse; do not mutate after first use.</summary>
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        WriteIndented = false,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
}