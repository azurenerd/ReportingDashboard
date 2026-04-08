using AgentSquad.Dashboard.Models;
using AgentSquad.Dashboard.Services;
using Microsoft.AspNetCore.Mvc;

namespace AgentSquad.Runner.Controllers;

[ApiController]
[Route("api")]
public class ProjectDataController : ControllerBase
{
    private readonly ProjectDataService _projectDataService;
    private readonly ILogger<ProjectDataController> _logger;

    public ProjectDataController(
        ProjectDataService projectDataService,
        ILogger<ProjectDataController> logger)
    {
        _projectDataService = projectDataService ?? throw new ArgumentNullException(nameof(projectDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("project-data")]
    public async Task<IActionResult> GetProjectData()
    {
        try
        {
            var data = await _projectDataService.LoadProjectDataAsync();
            return Ok(data);
        }
        catch (DataLoadException ex)
        {
            _logger.LogError(ex, "Error loading project data");
            return BadRequest(new { error = "Invalid project data", message = ex.Message });
        }
    }
}