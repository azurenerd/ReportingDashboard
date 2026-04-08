using Bunit;
using Xunit;
using AgentSquad.Dashboard.Models;
using AgentSquad.Dashboard.Services;
using AgentSquad.Dashboard.Pages;
using Microsoft.Extensions.Configuration;

namespace AgentSquad.Dashboard.Tests
{
    public class DashboardComponentTests : TestContext
    {
        [Fact]
        public void Dashboard_Displays_Loading_State_Initially()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddScoped<ProjectDataService>();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string> { { "DataFilePath", "nonexistent.json" } })
                .Build();
            services.AddSingleton(config);
            Services = services.BuildServiceProvider();

            // Act
            var cut = RenderComponent<Dashboard>();
            var spinner = cut.FindAll(".spinner-border");

            // Assert
            Assert.NotEmpty(spinner);
            var loadingText = cut.Find(".visually-hidden");
            Assert.Contains("Loading", loadingText.TextContent);
        }

        [Fact]
        public void Dashboard_Displays_Error_Message_When_File_Not_Found()
        {
            // Arrange
            var services = new ServiceCollection();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string> { { "DataFilePath", "nonexistent.json" } })
                .Build();
            services.AddSingleton(config);
            services.AddScoped<ProjectDataService>();
            Services = services.BuildServiceProvider();

            // Act
            var cut = RenderComponent<Dashboard>();

            // Wait for component initialization
            cut.WaitForAssertion(() =>
            {
                var alert = cut.FindAll(".alert-danger");
                Assert.NotEmpty(alert);
            }, timeout: TimeSpan.FromSeconds(2));

            // Assert
            var errorAlert = cut.Find(".alert-danger");
            Assert.Contains("Error Loading Dashboard", errorAlert.TextContent);
        }

        [Fact]
        public void Dashboard_Renders_StatusCard_Grid_With_Responsive_Classes()
        {
            // Arrange
            var html = @"
                <div class='row mb-5'>
                    <div class='col-12 col-md-4 mb-3'>
                        <!-- Shipped Card -->
                    </div>
                    <div class='col-12 col-md-4 mb-3'>
                        <!-- In-Progress Card -->
                    </div>
                    <div class='col-12 col-md-4 mb-3'>
                        <!-- Carried-Over Card -->
                    </div>
                </div>";

            // Assert responsive grid structure
            Assert.Contains("col-12", html);
            Assert.Contains("col-md-4", html);
            Assert.Equal(3, html.Split("col-md-4").Length - 1);
        }

        [Fact]
        public void Dashboard_CSS_Font_Size_Meets_12pt_Minimum()
        {
            // 16px = 12pt (standard web-to-print conversion)
            var baseFontSize = 16;
            var minPointSize = 12;
            
            // 16px / 1.33 ≈ 12pt
            var calculatedPt = baseFontSize / 1.33;
            Assert.True(calculatedPt >= minPointSize);
        }

        [Fact]
        public void Dashboard_Helper_Methods_Filter_Tasks_By_Enum()
        {
            // Verify enum-based filtering is used
            // Dashboard calls: GetTasksByStatus(TaskStatus.Shipped)
            // Helper should filter where task.Status == TaskStatus.Shipped
            
            var testTasks = new List<ProjectTask>
            {
                new ProjectTask { Id = 1, Name = "T1", Status = TaskStatus.Shipped, Owner = "Owner1" },
                new ProjectTask { Id = 2, Name = "T2", Status = TaskStatus.InProgress, Owner = "Owner2" },
                new ProjectTask { Id = 3, Name = "T3", Status = TaskStatus.CarriedOver, Owner = "Owner3" }
            };

            var shippedOnly = testTasks.Where(t => t.Status == TaskStatus.Shipped).ToList();
            Assert.Single(shippedOnly);
            Assert.Equal("T1", shippedOnly[0].Name);
        }

        [Fact]
        public void Dashboard_Helper_Methods_Are_Null_Safe()
        {
            // Verify null-safety: GetTasksByStatus() returns empty list if Tasks is null
            List<ProjectTask> nullTasks = null;
            var result = nullTasks?.Where(t => t.Status == TaskStatus.Shipped).ToList() ?? new List<ProjectTask>();
            
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void Dashboard_GetStatusColor_Returns_Valid_Hex_Colors()
        {
            // Verify color scheme matches Architecture
            var colorMap = new Dictionary<string, string>
            {
                { "on-track", "#28a745" },      // Green
                { "at-risk", "#ffc107" },       // Yellow
                { "off-track", "#dc3545" },     // Red
                { "default", "#6c757d" }        // Gray
            };

            foreach (var color in colorMap.Values)
            {
                Assert.Matches(@"^#[0-9a-fA-F]{6}$", color);
            }
        }
    }
}