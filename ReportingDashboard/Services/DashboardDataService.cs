using System.Globalization;
using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public class DashboardDataService
{
    private const double SvgWidth = 1560.0;
    private const double SvgHeight = 185.0;
    private const double TopReserved = 28.0;

    private DateOnly _rangeStart;
    private DateOnly _rangeEnd;
    private DateOnly _reportDate;

    public DashboardData? Data { get; private set; }
    public string? ErrorMessage { get; private set; }
    public bool HasError => ErrorMessage is not null;

    public DashboardDataService(IWebHostEnvironment env)
    {
        LoadData(env);
    }

    private void LoadData(IWebHostEnvironment env)
    {
        var jsonPath = Path.Combine(env.WebRootPath, "data", "data.json");

        if (!File.Exists(jsonPath))
        {
            ErrorMessage = "Unable to load dashboard data. Please check data.json.";
            return;
        }

        try
        {
            var json = File.ReadAllText(jsonPath);
            Data = JsonSerializer.Deserialize<DashboardData>(json);

            if (Data is null)
            {
                ErrorMessage = "Unable to load dashboard data. Please check data.json.";
                return;
            }

            _rangeStart = DateOnly.Parse(Data.TimelineRange.Start, CultureInfo.InvariantCulture);
            _rangeEnd = DateOnly.Parse(Data.TimelineRange.End, CultureInfo.InvariantCulture);
            _reportDate = DateOnly.Parse(Data.ReportDate, CultureInfo.InvariantCulture);
        }
        catch (JsonException ex)
        {
            Data = null;
            ErrorMessage = $"Unable to load dashboard data. JSON parse error: {ex.Message}";
        }
        catch (FormatException ex)
        {
            Data = null;
            ErrorMessage = $"Unable to load dashboard data. Date format error: {ex.Message}";
        }
        catch (Exception ex)
        {
            Data = null;
            ErrorMessage = $"Unable to load dashboard data. Error: {ex.Message}";
        }
    }

    public double DateToX(DateOnly date)
    {
        double totalDays = _rangeEnd.DayNumber - _rangeStart.DayNumber;
        if (totalDays <= 0) return 0;
        double dayOffset = date.DayNumber - _rangeStart.DayNumber;
        return (dayOffset / totalDays) * SvgWidth;
    }

    public double GetNowX()
    {
        return DateToX(_reportDate);
    }

    public double GetWorkstreamY(int index, int totalWorkstreams)
    {
        double usableHeight = SvgHeight - TopReserved;
        double spacing = usableHeight / (totalWorkstreams + 1);
        return TopReserved + spacing * (index + 1);
    }

    public string GetCurrentMonthLabel()
    {
        return _reportDate.ToString("MMMM", CultureInfo.InvariantCulture);
    }
}