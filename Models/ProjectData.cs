using System.ComponentModel.DataAnnotations;

namespace AgentSquad.Runner.Models;

public record ProjectData(
    [property: Required] ProjectInfo Project,
    [property: Required] Milestone[] Milestones,
    [property: Required] ProjectTask[] Tasks
);