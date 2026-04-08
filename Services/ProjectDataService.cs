using System.Text.Json;
using AgentSquad.Services.Models;

namespace AgentSquad.Services;

public class ProjectDataService
{
    public async Task<ProjectData> LoadProjectDataAsync(string jsonFilePath)
    {
        try
        {
            if (!File.Exists(jsonFilePath))
            {
                return GetDefaultProjectData();
            }

            var json = await File.ReadAllTextAsync(jsonFilePath);
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
            var projectData = JsonSerializer.Deserialize<ProjectData>(json, options);
            return projectData ?? GetDefaultProjectData();
        }
        catch (Exception)
        {
            return GetDefaultProjectData();
        }
    }

    public ProjectData GetDefaultProjectData()
    {
        var today = DateTime.Now;
        return new ProjectData
        {
            ProjectName = "Executive Dashboard Project",
            Description = "Real-time project status reporting dashboard",
            StartDate = today.AddDays(-60),
            EndDate = today.AddDays(120),
            TotalTasks = 24,
            CompletedTasks = 8,
            Milestones = new List<Milestone>
            {
                new Milestone
                {
                    Id = "m1",
                    Name = "Project Kickoff",
                    TargetDate = today.AddDays(-30),
                    ActualDate = today.AddDays(-30),
                    Status = MilestoneStatus.Completed,
                    CompletionPercentage = 100
                },
                new Milestone
                {
                    Id = "m2",
                    Name = "Phase 1 Design Review",
                    TargetDate = today.AddDays(-10),
                    ActualDate = today.AddDays(-10),
                    Status = MilestoneStatus.Completed,
                    CompletionPercentage = 100
                },
                new Milestone
                {
                    Id = "m3",
                    Name = "Development Sprint 1",
                    TargetDate = today.AddDays(15),
                    Status = MilestoneStatus.InProgress,
                    CompletionPercentage = 65
                },
                new Milestone
                {
                    Id = "m4",
                    Name = "Quality Assurance & Testing",
                    TargetDate = today.AddDays(45),
                    Status = MilestoneStatus.Pending,
                    CompletionPercentage = 0
                }
            }
        };
    }
}