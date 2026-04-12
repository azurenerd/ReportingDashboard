using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services
{
    /// <summary>
    /// Service responsible for loading, deserializing, validating, and caching project dashboard data from JSON file.
    /// Implements the IReportService interface and provides async data loading with comprehensive error handling.
    /// </summary>
    public interface IReportService
    {
        /// <summary>
        /// Asynchronously loads project dashboard data from the configured data.json file.
        /// Deserializes the JSON, validates the structure, sanitizes string values to prevent XSS,
        /// and caches the result in memory for subsequent calls.
        /// </summary>
        /// <returns>A Task that completes with a fully populated and validated ReportData object.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the data.json file is not found.</exception>
        /// <exception cref="JsonException">Thrown when the data.json file contains invalid JSON syntax.</exception>
        /// <exception cref="ArgumentException">Thrown when required data fields are missing or invalid.</exception>
        /// <exception cref="FormatException">Thrown when date or color fields have invalid format.</exception>
        Task<ReportData> LoadReportDataAsync();

        /// <summary>
        /// Gets the currently cached report data without re-reading the file.
        /// </summary>
        ReportData CurrentData { get; }
    }

    /// <summary>
    /// Implementation of IReportService for loading and managing project dashboard data.
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ReportService> _logger;
        private ReportData _cachedData;
        private readonly object _cacheLock = new object();

        /// <summary>
        /// Initializes a new instance of the ReportService class with required dependencies.
        /// </summary>
        public ReportService(
            IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration,
            ILogger<ReportService> logger)
        {
            _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cachedData = null;
        }

        /// <summary>
        /// Gets the currently cached report data without re-reading the file.
        /// Returns null if data has not been loaded yet.
        /// </summary>
        public ReportData CurrentData
        {
            get
            {
                lock (_cacheLock)
                {
                    return _cachedData;
                }
            }
        }

        /// <summary>
        /// Asynchronously loads project dashboard data from the configured data.json file.
        /// Deserializes the JSON, validates the structure, sanitizes string values to prevent XSS,
        /// and caches the result in memory for subsequent calls.
        /// 
        /// Deserialized data is cached after first successful load. Subsequent calls return the cached instance
        /// without re-reading the file.
        /// </summary>
        public async Task<ReportData> LoadReportDataAsync()
        {
            // Return cached data if already loaded
            lock (_cacheLock)
            {
                if (_cachedData != null)
                {
                    _logger.LogInformation("Returning cached report data");
                    return _cachedData;
                }
            }

            try
            {
                // Resolve file path from configuration
                string dataFilePath = ResolveDataFilePath();
                _logger.LogInformation("Loading report data from: {DataFilePath}", dataFilePath);

                // Read file content
                string jsonContent = await File.ReadAllTextAsync(dataFilePath);

                if (string.IsNullOrWhiteSpace(jsonContent))
                {
                    throw new JsonException("Data file is empty. Please ensure data.json contains valid JSON data.");
                }

                // Deserialize JSON
                ReportData reportData = DeserializeJson(jsonContent);

                // Validate data structure
                ValidateReportData(reportData);

                // Sanitize all string fields to prevent XSS
                SanitizeData(reportData);

                // Cache the loaded data
                lock (_cacheLock)
                {
                    _cachedData = reportData;
                }

                _logger.LogInformation("Report data loaded and cached successfully. " +
                    "Milestones: {MilestoneCount}, StatusRows: {StatusRowCount}",
                    reportData.Milestones?.Count ?? 0,
                    reportData.StatusRows?.Count ?? 0);

                return reportData;
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError(ex, "Data file not found");
                throw new FileNotFoundException(
                    $"Data file not found at path: {ex.FileName}. Please ensure data.json exists in the same directory as the executable.",
                    ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Invalid JSON in data file");
                throw new JsonException(
                    $"Invalid JSON in data file: {ex.Message}. Please verify data.json contains valid JSON syntax.",
                    ex);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Data validation failed");
                throw;
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, "Date or format validation failed");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error loading report data");
                throw new InvalidOperationException(
                    "An unexpected error occurred while loading the dashboard data. Please check the application logs.",
                    ex);
            }
        }

        /// <summary>
        /// Resolves the full file path to the data.json file.
        /// </summary>
        private string ResolveDataFilePath()
        {
            string configuredPath = _configuration["DataFilePath"] ?? "data/data.json";
            string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, configuredPath);
            fullPath = Path.GetFullPath(fullPath);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"Data file not found at path: {fullPath}", fullPath);
            }

            return fullPath;
        }

        /// <summary>
        /// Deserializes JSON content into a strongly-typed ReportData object.
        /// </summary>
        private ReportData DeserializeJson(string jsonContent)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                ReportData data = JsonSerializer.Deserialize<ReportData>(jsonContent, options);

                if (data == null)
                {
                    throw new JsonException("Failed to deserialize JSON: result is null");
                }

                return data;
            }
            catch (JsonException ex) when (!(ex.Message.Contains("Failed to deserialize")))
            {
                throw new JsonException($"Invalid JSON syntax: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Validates the structure and content of loaded ReportData.
        /// </summary>
        private void ValidateReportData(ReportData reportData)
        {
            if (reportData == null)
            {
                throw new ArgumentException("ReportData cannot be null");
            }

            // Validate ReportTitle (required, non-empty, max 255 chars)
            if (string.IsNullOrWhiteSpace(reportData.ReportTitle))
            {
                throw new ArgumentException("ReportTitle is required and cannot be empty");
            }

            if (reportData.ReportTitle.Length > 255)
            {
                throw new ArgumentException("ReportTitle cannot exceed 255 characters");
            }

            // Ensure collections are not null (apply defaults)
            reportData.Milestones ??= new List<Milestone>();
            reportData.StatusRows ??= new List<StatusRow>();

            // Validate Milestones
            var milestoneIds = new HashSet<string>();
            foreach (var milestone in reportData.Milestones)
            {
                if (milestone == null)
                {
                    throw new ArgumentException("Milestone cannot be null");
                }

                if (string.IsNullOrWhiteSpace(milestone.Id))
                {
                    throw new ArgumentException("Milestone.Id is required and cannot be empty");
                }

                if (milestone.Id.Length > 50)
                {
                    throw new ArgumentException($"Milestone.Id cannot exceed 50 characters: {milestone.Id}");
                }

                if (!milestoneIds.Add(milestone.Id))
                {
                    throw new ArgumentException($"Duplicate Milestone.Id found: {milestone.Id}");
                }

                if (string.IsNullOrWhiteSpace(milestone.Title))
                {
                    throw new ArgumentException($"Milestone '{milestone.Id}' must have a non-empty Title");
                }

                if (milestone.Title.Length > 100)
                {
                    throw new ArgumentException($"Milestone '{milestone.Id}' Title cannot exceed 100 characters");
                }

                if (string.IsNullOrWhiteSpace(milestone.Color))
                {
                    throw new ArgumentException($"Milestone '{milestone.Id}' must have a Color");
                }

                if (!IsValidHexColor(milestone.Color))
                {
                    throw new FormatException(
                        $"Milestone '{milestone.Id}' Color must be valid hex format (e.g., #0078D4). Got: {milestone.Color}");
                }

                if (milestone.StartDate == default)
                {
                    throw new ArgumentException($"Milestone '{milestone.Id}' must have a valid StartDate");
                }

                milestone.Checkpoints ??= new List<Checkpoint>();

                foreach (var checkpoint in milestone.Checkpoints)
                {
                    if (checkpoint == null)
                    {
                        throw new ArgumentException($"Milestone '{milestone.Id}' has null Checkpoint");
                    }

                    if (checkpoint.Date == default)
                    {
                        throw new ArgumentException(
                            $"Milestone '{milestone.Id}' has Checkpoint with invalid Date");
                    }

                    if (checkpoint.Date < milestone.StartDate)
                    {
                        _logger.LogWarning("Milestone '{MilestoneId}' has Checkpoint dated before StartDate",
                            milestone.Id);
                    }
                }

                if (milestone.PocMilestone != null)
                {
                    if (milestone.PocMilestone.Date == default)
                    {
                        throw new ArgumentException(
                            $"Milestone '{milestone.Id}' PocMilestone has invalid Date");
                    }

                    if (string.IsNullOrWhiteSpace(milestone.PocMilestone.Color))
                    {
                        _logger.LogWarning("Milestone '{MilestoneId}' PocMilestone has no Color", milestone.Id);
                    }
                    else if (!IsValidHexColor(milestone.PocMilestone.Color))
                    {
                        throw new FormatException(
                            $"Milestone '{milestone.Id}' PocMilestone Color must be valid hex format. Got: {milestone.PocMilestone.Color}");
                    }
                }

                if (milestone.ProductionRelease != null)
                {
                    if (milestone.ProductionRelease.Date == default)
                    {
                        throw new ArgumentException(
                            $"Milestone '{milestone.Id}' ProductionRelease has invalid Date");
                    }

                    if (string.IsNullOrWhiteSpace(milestone.ProductionRelease.Color))
                    {
                        _logger.LogWarning("Milestone '{MilestoneId}' ProductionRelease has no Color", milestone.Id);
                    }
                    else if (!IsValidHexColor(milestone.ProductionRelease.Color))
                    {
                        throw new FormatException(
                            $"Milestone '{milestone.Id}' ProductionRelease Color must be valid hex format. Got: {milestone.ProductionRelease.Color}");
                    }
                }
            }

            // Validate StatusRows
            foreach (var row in reportData.StatusRows)
            {
                if (row == null)
                {
                    throw new ArgumentException("StatusRow cannot be null");
                }

                if (string.IsNullOrWhiteSpace(row.Category))
                {
                    throw new ArgumentException("StatusRow.Category is required and cannot be empty");
                }

                var validCategories = new[] { "Shipped", "InProgress", "Carryover", "Blockers" };
                if (!validCategories.Contains(row.Category))
                {
                    throw new ArgumentException(
                        $"StatusRow.Category '{row.Category}' is invalid. Must be one of: {string.Join(", ", validCategories)}");
                }

                if (string.IsNullOrWhiteSpace(row.HeaderCssClass))
                {
                    throw new ArgumentException($"StatusRow '{row.Category}' requires HeaderCssClass");
                }

                if (string.IsNullOrWhiteSpace(row.CellCssClass))
                {
                    throw new ArgumentException($"StatusRow '{row.Category}' requires CellCssClass");
                }

                row.Items ??= new List<StatusItem>();

                foreach (var item in row.Items)
                {
                    if (item == null)
                    {
                        throw new ArgumentException($"StatusRow '{row.Category}' has null Item");
                    }

                    if (string.IsNullOrWhiteSpace(item.Month))
                    {
                        throw new ArgumentException($"StatusRow '{row.Category}' item missing Month");
                    }
                }
            }
        }

        /// <summary>
        /// Sanitizes all string fields to prevent XSS vulnerabilities.
        /// </summary>
        private void SanitizeData(ReportData data)
        {
            if (!string.IsNullOrWhiteSpace(data.ReportTitle))
            {
                data.ReportTitle = WebUtility.HtmlEncode(data.ReportTitle);
            }

            if (!string.IsNullOrWhiteSpace(data.Subtitle))
            {
                data.Subtitle = WebUtility.HtmlEncode(data.Subtitle);
            }

            if (!string.IsNullOrWhiteSpace(data.AdoBacklogUrl))
            {
                if (!Uri.TryCreate(data.AdoBacklogUrl, UriKind.Absolute, out _))
                {
                    throw new FormatException($"AdoBacklogUrl is not a valid URI: {data.AdoBacklogUrl}");
                }
            }

            if (data.Milestones != null)
            {
                foreach (var milestone in data.Milestones)
                {
                    if (!string.IsNullOrWhiteSpace(milestone.Title))
                    {
                        milestone.Title = WebUtility.HtmlEncode(milestone.Title);
                    }

                    if (milestone.Checkpoints != null)
                    {
                        foreach (var checkpoint in milestone.Checkpoints)
                        {
                            if (!string.IsNullOrWhiteSpace(checkpoint.Label))
                            {
                                checkpoint.Label = WebUtility.HtmlEncode(checkpoint.Label);
                            }
                        }
                    }

                    if (milestone.PocMilestone != null && !string.IsNullOrWhiteSpace(milestone.PocMilestone.Label))
                    {
                        milestone.PocMilestone.Label = WebUtility.HtmlEncode(milestone.PocMilestone.Label);
                    }

                    if (milestone.ProductionRelease != null && !string.IsNullOrWhiteSpace(milestone.ProductionRelease.Label))
                    {
                        milestone.ProductionRelease.Label = WebUtility.HtmlEncode(milestone.ProductionRelease.Label);
                    }
                }
            }

            if (data.StatusRows != null)
            {
                foreach (var row in data.StatusRows)
                {
                    if (row.Items != null)
                    {
                        foreach (var item in row.Items)
                        {
                            if (!string.IsNullOrWhiteSpace(item.Value))
                            {
                                item.Value = WebUtility.HtmlEncode(item.Value);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Validates hex color format (#RRGGBB).
        /// </summary>
        private bool IsValidHexColor(string color)
        {
            if (string.IsNullOrWhiteSpace(color))
            {
                return false;
            }

            if (!color.StartsWith("#") || color.Length != 7)
            {
                return false;
            }

            return Regex.IsMatch(color, @"^#[0-9A-Fa-f]{6}$");
        }
    }
}