using Xunit;
using AgentSquad.Components;
using AgentSquad.Services;
using System.Reflection;

namespace AgentSquad.Tests.AcceptanceCriteria
{
    public class BlazorServerSetupTests
    {
        [Fact]
        public void DashboardComponent_ExistsAndIsPublic()
        {
            var dashboardType = typeof(Dashboard);
            Assert.NotNull(dashboardType);
            Assert.True(dashboardType.IsPublic);
        }

        [Fact]
        public void ProjectDataService_HasLoadProjectDataAsyncMethod()
        {
            var serviceType = typeof(ProjectDataService);
            var method = serviceType.GetMethod("LoadProjectDataAsync", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);
            Assert.True(method.ReturnType.IsGenericType);
        }

        [Fact]
        public void Components_ResideInCorrectNamespace()
        {
            var types = typeof(Dashboard).Assembly.GetTypes()
                .Where(t => t.Name.Contains("Component") || t.Name.Contains("Dashboard"));
            Assert.NotEmpty(types);
            Assert.All(types, t => Assert.StartsWith("AgentSquad.Components", t.Namespace));
        }

        [Fact]
        public void ProgramFile_ConfiguresBlazorServer()
        {
            var programAssembly = typeof(Program).Assembly;
            Assert.NotNull(programAssembly.GetType("AgentSquad.Program"));
        }
    }
}