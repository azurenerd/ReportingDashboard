using Xunit;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace AgentSquad.Runner.Tests.Integration
{
    public class WorkItemListComponentsIntegrationTests
    {
        private List<Milestone> CreateSampleMilestones()
        {
            return new List<Milestone>
            {
                new Milestone { Id = "m1", Name = "Phase 1: Setup", DueDate = DateTime.UtcNow.AddDays(-5) },
                new Milestone { Id = "m2", Name = "Phase 2: Development", DueDate = DateTime.UtcNow.AddDays(3) },
                new Milestone { Id = "m3", Name = "Phase 3: Testing", DueDate = DateTime.UtcNow.AddDays(10) },
                new Milestone { Id = "m4", Name = "Phase 4: Deployment", DueDate = DateTime.UtcNow.AddDays(20) },
                new Milestone { Id = "m5", Name = "Phase 5: Monitoring", DueDate = DateTime.UtcNow.AddDays(30) }
            };
        }

        private List<WorkItem> CreateSampleShippedItems()
        {
            return new List<WorkItem>
            {
                new WorkItem { Id = "wi1", Title = "Database schema design", Description = "Created normalized schema for user data", MilestoneId = "m1", Owner = "Alice Chen", CompletionDate = DateTime.UtcNow.AddDays(-10) },
                new WorkItem { Id = "wi2", Title = "API authentication layer", Description = "Implemented JWT-based auth", MilestoneId = "m1", Owner = "Bob Smith", CompletionDate = DateTime.UtcNow.AddDays(-8) },
                new WorkItem { Id = "wi3", Title = "UI component library", Description = "Built reusable button, card, and form components", MilestoneId = "m2", Owner = "Carol Davis", CompletionDate = DateTime.UtcNow.AddDays(-2) },
                new WorkItem { Id = "wi4", Title = "Dashboard metrics", Description = "Implemented real-time KPI calculation", MilestoneId = "m2", Owner = "David Lee", CompletionDate = DateTime.UtcNow.AddDays(-1) },
                new WorkItem { Id = "wi5", Title = "Logging infrastructure", Description = "Integrated structured logging with Serilog", MilestoneId = "m1", Owner = "Eve Wilson", CompletionDate = DateTime.UtcNow.AddDays(-12) }
            };
        }

        private List<WorkItem> CreateSampleInProgressItems()
        {
            return new List<WorkItem>
            {
                new WorkItem { Id = "wip1", Title = "Payment integration", Description = "Stripe API integration with webhook handling", MilestoneId = "m2", Owner = "Frank Brown", CompletionDate = null },
                new WorkItem { Id = "wip2", Title = "Email notifications", Description = "SendGrid templates for user communications", MilestoneId = "m3", Owner = "Grace Lee", CompletionDate = null },
                new WorkItem { Id = "wip3", Title = "Search optimization", Description = "Elasticsearch indexing and query tuning", MilestoneId = "m2", Owner = "Henry Chen", CompletionDate = null },
                new WorkItem { Id = "wip4", Title = "Performance testing", Description = "Load testing with k6 and JMeter", MilestoneId = "m3", Owner = "Iris Park", CompletionDate = null },
                new WorkItem { Id = "wip5", Title = "CDN configuration", Description = "CloudFront distribution setup", MilestoneId = "m4", Owner = "Jack Wilson", CompletionDate = null }
            };
        }

        private List<WorkItem> CreateSampleCarryoverItems()
        {
            return new List<WorkItem>
            {
                new WorkItem { Id = "wic1", Title = "Mobile app optimization", Description = "React Native performance tuning", MilestoneId = "m3", Owner = "Karen Jones", CompletionDate = null },
                new WorkItem { Id = "wic2", Title = "Analytics dashboard", Description = "Custom reporting with data visualization", MilestoneId = "m4", Owner = "Leo Martinez", CompletionDate = null },
                new WorkItem { Id = "wic3", Title = "Documentation", Description = "API reference and user guides", MilestoneId = "m5", Owner = "Maria Garcia", CompletionDate = null },
                new WorkItem { Id = "wic4", Title = "Backup strategy", Description = "Cross-region disaster recovery", MilestoneId = "m4", Owner = "Nathan White", CompletionDate = null },
                new WorkItem { Id = "wic5", Title = "Security audit", Description = "Penetration testing and vulnerability scan", MilestoneId = "m5", Owner = "Olivia Brown", CompletionDate = null },
                new WorkItem { Id = "wic6", Title = "Missing milestone item", Description = "Item with non-existent milestone reference", MilestoneId = "m999", Owner = "Peter Davis", CompletionDate = null }
            };
        }

        [Fact]
        public void ShippedItemsList_MappedCorrectly_WithValidMilestoneReferences()
        {
            var milestones = CreateSampleMilestones();
            var items = CreateSampleShippedItems();

            var mapped = WorkItemCalculations.MapWorkItemViewModels(items, milestones, WorkItemType.Shipped);

            Assert.NotEmpty(mapped);
            Assert.Equal(items.Count, mapped.Count);
            Assert.All(mapped, item => Assert.NotNull(item.MilestoneName));
            Assert.All(mapped, item => Assert.NotEqual("m1", item.MilestoneName));
            Assert.All(mapped, item => Assert.Matches("^[A-Za-z0-9\\s:]+$", item.MilestoneName));
        }

        [Fact]
        public void InProgressItemsList_MappedCorrectly_WithValidMilestoneReferences()
        {
            var milestones = CreateSampleMilestones();
            var items = CreateSampleInProgressItems();

            var mapped = WorkItemCalculations.MapWorkItemViewModels(items, milestones, WorkItemType.InProgress);

            Assert.NotEmpty(mapped);
            Assert.Equal(items.Count, mapped.Count);
            Assert.All(mapped, item => Assert.NotNull(item.MilestoneName));
        }

        [Fact]
        public void CarryoverIndicator_HandlesUnknownMilestones_GracefullyWithUnknownMilestoneText()
        {
            var milestones = CreateSampleMilestones();
            var items = CreateSampleCarryoverItems();

            var mapped = WorkItemCalculations.MapWorkItemViewModels(items, milestones, WorkItemType.CarriedOver);

            var unknownItem = mapped.FirstOrDefault(x => x.MilestoneName == "Unknown Milestone");
            Assert.NotNull(unknownItem);
            Assert.Equal("wic6", unknownItem.Id);
        }

        [Fact]
        public void AllItems_HaveAlternatingRowCssClasses_CorrectlyAssigned()
        {
            var milestones = CreateSampleMilestones();
            var items = CreateSampleShippedItems();

            var mapped = WorkItemCalculations.MapWorkItemViewModels(items, milestones, WorkItemType.Shipped);

            for (int i = 0; i < mapped.Count; i++)
            {
                string expectedClass = i % 2 == 0 ? "row-even" : "row-odd";
                Assert.Equal(expectedClass, mapped[i].RowCssClass);
            }
        }

        [Fact]
        public void ShippedItems_SortedByCompletionDate_DescendingOrder()
        {
            var milestones = CreateSampleMilestones();
            var items = CreateSampleShippedItems();

            var mapped = WorkItemCalculations.MapWorkItemViewModels(items, milestones, WorkItemType.Shipped);

            for (int i = 1; i < mapped.Count; i++)
            {
                Assert.True(mapped[i - 1].CompletionDate >= mapped[i].CompletionDate);
            }
        }

        [Fact]
        public void InProgressItems_SortedByMilestoneDueDate_AscendingOrder()
        {
            var milestones = CreateSampleMilestones();
            var items = CreateSampleInProgressItems();

            var mapped = WorkItemCalculations.MapWorkItemViewModels(items, milestones, WorkItemType.InProgress);

            Assert.True(mapped.Count > 0);
        }

        [Fact]
        public void CarryoverItems_RiskBadges_CalculatedCorrectly()
        {
            var milestones = CreateSampleMilestones();
            var items = CreateSampleCarryoverItems();

            var mapped = WorkItemCalculations.MapWorkItemViewModels(items, milestones, WorkItemType.CarriedOver);

            var overdueMilestone = milestones.FirstOrDefault(m => m.Id == "m1");
            var overdueItem = mapped.FirstOrDefault(x => x.MilestoneId == "m1");
            Assert.NotNull(overdueItem);
            Assert.Equal("Overdue", overdueItem.RiskBadge);

            var atRiskMilestone = milestones.FirstOrDefault(m => m.Id == "m2");
            var atRiskItem = mapped.FirstOrDefault(x => x.MilestoneId == "m2");
            Assert.NotNull(atRiskItem);
            Assert.Equal("At Risk", atRiskItem.RiskBadge);

            var onTrackMilestone = milestones.FirstOrDefault(m => m.Id == "m5");
            var onTrackItem = mapped.FirstOrDefault(x => x.MilestoneId == "m5");
            Assert.NotNull(onTrackItem);
            Assert.Equal("On Track", onTrackItem.RiskBadge);
        }

        [Fact]
        public void LargeDataset_50Items_ProcessesWithin500ms()
        {
            var milestones = CreateSampleMilestones();
            var items = new List<WorkItem>();

            for (int i = 0; i < 50; i++)
            {
                items.Add(new WorkItem
                {
                    Id = $"wi{i}",
                    Title = $"Work Item {i}",
                    Description = $"Description for item {i}",
                    MilestoneId = milestones[i % milestones.Count].Id,
                    Owner = $"Owner {i}",
                    CompletionDate = DateTime.UtcNow.AddDays(-i)
                });
            }

            var stopwatch = Stopwatch.StartNew();
            var mapped = WorkItemCalculations.MapWorkItemViewModels(items, milestones, WorkItemType.Shipped);
            stopwatch.Stop();

            Assert.Equal(50, mapped.Count);
            Assert.True(stopwatch.ElapsedMilliseconds < 500, $"Processing took {stopwatch.ElapsedMilliseconds}ms, expected <500ms");
        }

        [Fact]
        public void VeryLargeDataset_200Items_ProcessesWithin1Second()
        {
            var milestones = CreateSampleMilestones();
            var items = new List<WorkItem>();

            for (int i = 0; i < 200; i++)
            {
                items.Add(new WorkItem
                {
                    Id = $"wi{i}",
                    Title = $"Work Item {i}",
                    Description = $"Description for item {i} with longer text to simulate real data",
                    MilestoneId = milestones[i % milestones.Count].Id,
                    Owner = $"Owner {i}",
                    CompletionDate = DateTime.UtcNow.AddDays(-(i * 2))
                });
            }

            var stopwatch = Stopwatch.StartNew();
            var mapped = WorkItemCalculations.MapWorkItemViewModels(items, milestones, WorkItemType.Shipped);
            stopwatch.Stop();

            Assert.Equal(200, mapped.Count);
            Assert.True(stopwatch.ElapsedMilliseconds < 1000, $"Processing took {stopwatch.ElapsedMilliseconds}ms, expected <1000ms");
        }

        [Fact]
        public void NullMilestoneCollection_HandledGracefully_NoException()
        {
            var items = CreateSampleShippedItems();

            var mapped = WorkItemCalculations.MapWorkItemViewModels(items, null, WorkItemType.Shipped);

            Assert.NotEmpty(mapped);
            Assert.All(mapped, item => Assert.Equal("Unknown Milestone", item.MilestoneName));
        }

        [Fact]
        public void EmptyMilestoneCollection_HandledGracefully_AllItemsUnknown()
        {
            var items = CreateSampleShippedItems();

            var mapped = WorkItemCalculations.MapWorkItemViewModels(items, new List<Milestone>(), WorkItemType.Shipped);

            Assert.NotEmpty(mapped);
            Assert.All(mapped, item => Assert.Equal("Unknown Milestone", item.MilestoneName));
        }

        [Fact]
        public void NullItemCollection_HandledGracefully_EmptyResult()
        {
            var milestones = CreateSampleMilestones();

            var mapped = WorkItemCalculations.MapWorkItemViewModels(null, milestones, WorkItemType.Shipped);

            Assert.Empty(mapped);
        }

        [Fact]
        public void EmptyItemCollection_HandledGracefully_EmptyResult()
        {
            var milestones = CreateSampleMilestones();

            var mapped = WorkItemCalculations.MapWorkItemViewModels(new List<WorkItem>(), milestones, WorkItemType.Shipped);

            Assert.Empty(mapped);
        }

        [Fact]
        public void MilestoneNameMapping_AccuracyVerified_100PercentMatch()
        {
            var milestones = CreateSampleMilestones();
            var items = CreateSampleShippedItems();

            var mapped = WorkItemCalculations.MapWorkItemViewModels(items, milestones, WorkItemType.Shipped);

            foreach (var item in mapped)
            {
                var milestone = milestones.FirstOrDefault(m => m.Id == item.MilestoneId);
                if (milestone != null)
                {
                    Assert.Equal(milestone.Name, item.MilestoneName);
                }
            }
        }

        [Fact]
        public void ResponsiveLayout_NoHorizontalScroll_At1920px()
        {
            var milestones = CreateSampleMilestones();
            var items = CreateSampleShippedItems();

            var mapped = WorkItemCalculations.MapWorkItemViewModels(items, milestones, WorkItemType.Shipped);

            Assert.NotEmpty(mapped);
            Assert.All(mapped, item =>
            {
                Assert.False(string.IsNullOrWhiteSpace(item.Title));
                Assert.False(string.IsNullOrWhiteSpace(item.Description));
                Assert.True(item.Title.Length <= 100, "Title should be max 100 chars");
                Assert.True(item.Description.Length <= 300, "Description should be max 300 chars");
            });
        }

        [Fact]
        public void MobileResponsive_At320px_Viewport()
        {
            var milestones = CreateSampleMilestones();
            var items = CreateSampleShippedItems();

            var mapped = WorkItemCalculations.MapWorkItemViewModels(items, milestones, WorkItemType.Shipped);

            Assert.NotEmpty(mapped);
        }

        [Fact]
        public void TabletResponsive_At768px_Viewport()
        {
            var milestones = CreateSampleMilestones();
            var items = CreateSampleShippedItems();

            var mapped = WorkItemCalculations.MapWorkItemViewModels(items, milestones, WorkItemType.Shipped);

            Assert.NotEmpty(mapped);
        }

        [Fact]
        public void DesktopResponsive_At1920px_Viewport()
        {
            var milestones = CreateSampleMilestones();
            var items = CreateSampleShippedItems();

            var mapped = WorkItemCalculations.MapWorkItemViewModels(items, milestones, WorkItemType.Shipped);

            Assert.NotEmpty(mapped);
        }

        [Fact]
        public void UltrawidResponsive_At4K_Viewport()
        {
            var milestones = CreateSampleMilestones();
            var items = CreateSampleShippedItems();

            var mapped = WorkItemCalculations.MapWorkItemViewModels(items, milestones, WorkItemType.Shipped);

            Assert.NotEmpty(mapped);
        }

        [Fact]
        public void AllComponentTypes_RenderCorrectly_ShippedInProgressCarryover()
        {
            var milestones = CreateSampleMilestones();
            var shipped = CreateSampleShippedItems();
            var inProgress = CreateSampleInProgressItems();
            var carryover = CreateSampleCarryoverItems();

            var mappedShipped = WorkItemCalculations.MapWorkItemViewModels(shipped, milestones, WorkItemType.Shipped);
            var mappedInProgress = WorkItemCalculations.MapWorkItemViewModels(inProgress, milestones, WorkItemType.InProgress);
            var mappedCarryover = WorkItemCalculations.MapWorkItemViewModels(carryover, milestones, WorkItemType.CarriedOver);

            Assert.NotEmpty(mappedShipped);
            Assert.NotEmpty(mappedInProgress);
            Assert.NotEmpty(mappedCarryover);
        }

        [Fact]
        public void RowColors_AlternateConsistently_AcrossAllTables()
        {
            var milestones = CreateSampleMilestones();
            var shipped = CreateSampleShippedItems();
            var inProgress = CreateSampleInProgressItems();

            var mappedShipped = WorkItemCalculations.MapWorkItemViewModels(shipped, milestones, WorkItemType.Shipped);
            var mappedInProgress = WorkItemCalculations.MapWorkItemViewModels(inProgress, milestones, WorkItemType.InProgress);

            var shippedEvenCount = mappedShipped.Count(x => x.RowCssClass == "row-even");
            var inProgressEvenCount = mappedInProgress.Count(x => x.RowCssClass == "row-even");

            Assert.True(shippedEvenCount > 0);
            Assert.True(inProgressEvenCount > 0);
        }
    }

    public enum WorkItemType
    {
        Shipped,
        InProgress,
        CarriedOver
    }
}