using System.ComponentModel.DataAnnotations;

namespace AgentSquad.Dashboard.Models;

public class ProjectData
{
    [Required]
    public ProjectInfo Project { get; set; } = new();

    [Required]
    public List<Milestone> Milestones { get; set; } = new();

    [Required]
    public ProgressData Progress { get; set; } = new();

    [Required]
    public MetricsData Metrics { get; set; } = new();
}

public class ProjectInfo
{
    [Required(ErrorMessage = "Project name is required")]
    [StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Project owner is required")]
    [StringLength(100, MinimumLength = 1)]
    public string Owner { get; set; } = string.Empty;

    [Required(ErrorMessage = "Project status is required")]
    [RegularExpression(@"^(On Track|At Risk|Blocked)$")]
    public string Status { get; set; } = "On Track";

    [Required]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    [StringLength(500)]
    public string? Description { get; set; }
}

public class Milestone
{
    [Required(ErrorMessage = "Milestone ID is required")]
    [StringLength(50, MinimumLength = 1)]
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Milestone name is required")]
    [StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateTime TargetDate { get; set; }

    [Required(ErrorMessage = "Milestone status is required")]
    [RegularExpression(@"^(Completed|In Progress|Not Started)$")]
    public string Status { get; set; } = "Not Started";

    [Required]
    [Range(0, 100)]
    public int PercentComplete { get; set; }
}

public class ProgressData
{
    [Required]
    public List<ProgressItem> Shipped { get; set; } = new();

    [Required]
    public List<ProgressItem> InProgress { get; set; } = new();

    [Required]
    public List<ProgressItem> CarriedOver { get; set; } = new();
}

public class ProgressItem
{
    [Required(ErrorMessage = "Item ID is required")]
    [StringLength(50, MinimumLength = 1)]
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Item title is required")]
    [StringLength(300, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Assignee { get; set; }

    [StringLength(300)]
    public string? Reason { get; set; }
}

public class MetricsData
{
    [Required]
    [Range(0, 1000)]
    public int TotalItems { get; set; }

    [Required]
    [Range(0, 1000)]
    public int ShippedCount { get; set; }

    [Required]
    [Range(0, 1000)]
    public int InProgressCount { get; set; }

    [Required]
    [Range(0, 1000)]
    public int CarriedOverCount { get; set; }

    public decimal ShippedPercentage =>
        TotalItems > 0 ? Math.Round((ShippedCount * 100m) / TotalItems, 2) : 0;

    public decimal InProgressPercentage =>
        TotalItems > 0 ? Math.Round((InProgressCount * 100m) / TotalItems, 2) : 0;

    public decimal CarriedOverPercentage =>
        TotalItems > 0 ? Math.Round((CarriedOverCount * 100m) / TotalItems, 2) : 0;
}