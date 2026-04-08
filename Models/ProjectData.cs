using System;
using System.Collections.Generic;

namespace AgentSquad.Models
{
    public class ProjectData
    {
        public string ProjectName { get; set; }
        public string ProjectDescription { get; set; }
        public DateTime ProjectStartDate { get; set; }
        public DateTime ProjectEndDate { get; set; }
        public int CompletionPercentage { get; set; }
        public List<Milestone> Milestones { get; set; }
        public List<TaskItem> Tasks { get; set; }
    }
}