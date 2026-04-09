using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using AgentSquad.Core.Configuration;
using AgentSquad.Core.Persistence;
using AgentSquad.Dashboard.Services;
using System.Net;

namespace AgentSquad.Runner.Tests.Integration
{
    [Trait("Category", "Integration")]
    public class ProgramStartupTests : IAsyncLifetime
    {
        private WebApplicationFactory<Program> _factory;

        public async Task InitializeAsync()
        {
            _factory = new WebApplicationFactory<Program>();
            await Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            _factory?.Dispose();
            await Task.CompletedTask;
        }

        [Fact]
        public void ServiceConfiguration_ResolvesAgentSquadConfig_Successfully()
        {
            using var scope = _factory.Services.CreateScope();
            var options = scope.ServiceProvider.GetService<Microsoft.Extensions.Options.IOptions<AgentSquadConfig>>();

            Assert.NotNull(options);
            Assert.NotNull(options.Value);
        }

        [Fact]
        public void ServiceConfiguration_ResolvesLimitsConfig_Successfully()
        {
            using var scope = _factory.Services.CreateScope();
            var options = scope.ServiceProvider.GetService<Microsoft.Extensions.Options.IOptions<LimitsConfig>>();

            Assert.NotNull(options);
        }

        [Fact]
        public void ServiceConfiguration_ResolvesDashboardDataService_AsSingleton()
        {
            var service1 = _factory.Services.GetRequiredService<DashboardDataService>();
            var service2 = _factory.Services.GetRequiredService<DashboardDataService>();

            Assert.Same(service1, service2);
        }

        [Fact]
        public void ServiceConfiguration_ResolvesConfigurationService_AsSingleton()
        {
            var service1 = _factory.Services.GetRequiredService<ConfigurationService>();
            var service2 = _factory.Services.GetRequiredService<ConfigurationService>();

            Assert.Same(service1, service2);
        }

        [Fact]
        public void ServiceConfiguration_ResolvesProjectDataService_AsScoped()
        {
            using var scope1 = _factory.Services.CreateScope();
            using var scope2 = _factory.Services.CreateScope();

            var service1 = scope1.ServiceProvider.GetRequiredService<ProjectDataService>();
            var service2 = scope2.ServiceProvider.GetRequiredService<ProjectDataService>();

            Assert.NotNull(service1);
            Assert.NotNull(service2);
            Assert.NotSame(service1, service2);
        }

        [Fact]
        public void ServiceConfiguration_ResolvesAgentStateStore_AsSingleton()
        {
            var store1 = _factory.Services.GetRequiredService<AgentStateStore>();
            var store2 = _factory.Services.GetRequiredService<AgentStateStore>();

            Assert.Same(store1, store2);
        }

        [Fact]
        public void ServiceConfiguration_ResolvesAgentMemoryStore_AsSingleton()
        {
            var store1 = _factory.Services.GetRequiredService<AgentMemoryStore>();
            var store2 = _factory.Services.GetRequiredService<AgentMemoryStore>();

            Assert.Same(store1, store2);
        }

        [Fact]
        public void ServiceConfiguration_ResolvesProjectFileManager_AsSingleton()
        {
            var manager1 = _factory.Services.GetRequiredService<ProjectFileManager>();
            var manager2 = _factory.Services.GetRequiredService<ProjectFileManager>();

            Assert.Same(manager1, manager2);
        }
    }
}