namespace AgentSquad.ReportingDashboard.Services;

using System.Text.Json;
using AgentSquad.ReportingDashboard.Models;

public class DataService : IDataService, IAsyncDisposable
{
	private readonly IConfiguration _configuration;
	private readonly string _dataFilePath;
	private FileSystemWatcher? _fileSystemWatcher;
	private CancellationTokenSource? _debounceCts;
	private readonly object _lockObject = new();

	public DashboardData CurrentData { get; private set; } = new();

	public event Func<Task>? OnDataChanged;

	public DataService(IConfiguration configuration)
	{
		_configuration = configuration;
		_dataFilePath = configuration["Dashboard:DataFilePath"] ?? "wwwroot/data.json";
	}

	public async Task InitializeAsync()
	{
		try
		{
			var absolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _dataFilePath);
			
			if (File.Exists(absolutePath))
			{
				await LoadDataAsync(absolutePath);
			}
			else
			{
				System.Diagnostics.Debug.WriteLine($"DataService: data.json not found at {absolutePath}; using empty defaults");
				CurrentData = GetDefaultData();
			}

			StartFileWatcher(absolutePath);
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"DataService: InitializeAsync error: {ex.Message}");
			CurrentData = GetDefaultData();
		}
	}

	public async Task<DashboardData> ReloadDataAsync()
	{
		try
		{
			var absolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _dataFilePath);
			
			if (File.Exists(absolutePath))
			{
				await LoadDataAsync(absolutePath);
			}
			else
			{
				System.Diagnostics.Debug.WriteLine("DataService: data.json not found during reload");
				CurrentData = GetDefaultData();
			}
		}
		catch (JsonException ex)
		{
			System.Diagnostics.Debug.WriteLine($"DataService: JSON parse error during reload: {ex.Message}");
		}
		catch (IOException ex)
		{
			System.Diagnostics.Debug.WriteLine($"DataService: IO error during reload: {ex.Message}");
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"DataService: Unexpected error during reload: {ex.Message}");
		}

		return CurrentData;
	}

	private async Task LoadDataAsync(string filePath)
	{
		var json = await File.ReadAllTextAsync(filePath);
		var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
		var data = JsonSerializer.Deserialize<DashboardData>(json, options);
		
		lock (_lockObject)
		{
			CurrentData = data ?? GetDefaultData();
		}
	}

	private void StartFileWatcher(string filePath)
	{
		var directory = Path.GetDirectoryName(filePath);
		var fileName = Path.GetFileName(filePath);

		if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName))
		{
			return;
		}

		_fileSystemWatcher = new FileSystemWatcher(directory, fileName)
		{
			NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
			EnableRaisingEvents = true
		};

		_fileSystemWatcher.Changed += (s, e) => OnFileChanged();
		_fileSystemWatcher.Error += (s, e) => System.Diagnostics.Debug.WriteLine($"FileSystemWatcher error: {e.GetException()?.Message}");
	}

	private void OnFileChanged()
	{
		lock (_lockObject)
		{
			_debounceCts?.Cancel();
			_debounceCts?.Dispose();
			_debounceCts = new CancellationTokenSource();
		}

		var token = _debounceCts.Token;

		_ = Task.Delay(500, token).ContinueWith(async _ =>
		{
			if (!token.IsCancellationRequested)
			{
				await ReloadDataAsync();
				if (OnDataChanged != null)
				{
					await OnDataChanged.Invoke();
				}
			}
		}, TaskScheduler.Default);
	}

	private static DashboardData GetDefaultData()
	{
		return new DashboardData
		{
			ProjectName = "Dashboard",
			ProjectStatus = "Unknown",
			Milestones = new(),
			ShippedItems = new(),
			InProgressItems = new(),
			CarryoverItems = new(),
			LastUpdated = DateTime.UtcNow
		};
	}

	public async ValueTask DisposeAsync()
	{
		_fileSystemWatcher?.Dispose();
		_debounceCts?.Cancel();
		_debounceCts?.Dispose();
		await Task.CompletedTask;
	}
}