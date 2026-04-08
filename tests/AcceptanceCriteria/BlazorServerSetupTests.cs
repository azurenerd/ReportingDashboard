using Xunit;
using AgentSquad.Components;
using AgentSquad.Services;
using AgentSquad.Data;
using System.Reflection;

namespace AgentSquad.Tests.AcceptanceCriteria
{
    public class BlazorServerSetupTests
    {
        [Fact]
        public void ProjectDataService_ExistsWithFileIoMethod()
        {
            var serviceType = typeof(ProjectDataService);
            var method = serviceType.GetMethod(
                "LoadProjectDataAsync",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                null,
                new[] { typeof(string) },
                null
            );
            Assert.NotNull(method);
        }

        [Fact]
        public void ProjectDataService_HasGetCachedDataMethod()
        {
            var serviceType = typeof(ProjectDataService);
            var method = serviceType.GetMethod(
                "GetCachedData",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance
            );
            Assert.NotNull(method);
        }

        [Fact]
        public void ProjectDataService_HasRefreshDataMethod()
        {
            var serviceType = typeof(ProjectDataService);
            var method = serviceType.GetMethod(
                "RefreshData",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance
            );
            Assert.NotNull(method);
        }

        [Fact]
        public void ProjectData_ModelHasRequiredProperties()
        {
            var props = typeof(ProjectData).GetProperties();
            Assert.Contains(props, p => p.Name == "Name");
            Assert.Contains(props, p => p.Name == "Status");
            Assert.Contains(props, p => p.Name == "CompletionPercentage");
            Assert.Contains(props, p => p.Name == "Milestones");
        }

        [Fact]
        public void Milestone_ModelHasRequiredProperties()
        {
            var props = typeof(Milestone).GetProperties();
            Assert.Contains(props, p => p.Name == "Name");
            Assert.Contains(props, p => p.Name == "StartDate");
            Assert.Contains(props, p => p.Name == "TargetDate");
        }

        [Fact]
        public void Dashboard_ComponentExists()
        {
            var dashboardType = typeof(Dashboard);
            Assert.NotNull(dashboardType);
            Assert.True(dashboardType.IsPublic);
        }
    }
}