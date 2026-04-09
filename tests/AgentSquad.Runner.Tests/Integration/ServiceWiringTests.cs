using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using AgentSquad.Core.Configuration;
using AgentSquad.Core.Messaging;
using AgentSquad.Core.Persistence;
using AgentSquad.Dashboard.Services;

namespace AgentSquad.Runner.Tests.Integration
{
    [Trait("Category", "Integration")]
    public class ServiceWiringTests : IAsyncLifetime
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
        public void DependencyInjection_AllCriticalServices_CanBeResolved()
        {
            using var scope = _factory.Services.CreateScope();
            var provider = scope.ServiceProvider;

            var configService = provider.GetRequiredService<ConfigurationService>();
            var projectDataService = provider.GetRequiredService<ProjectDataService>();
            var dashboardService = provider.GetRequiredService<DashboardDataService>();

            Assert.NotNull(configService);
            Assert.NotNull(projectDataService);
            Assert.NotNull(dashboardService);
        }

        [Fact]
        public void DependencyInjection_SingletonServices_RetainInstances()
        {
            var service1 = _factory.Services.GetRequiredService<ConfigurationService>();
            var service2 = _factory.Services.GetRequiredService<ConfigurationService>();

            Assert.Same(service1, service2);
        }

        [Fact]
        public void DependencyInjection_ScopedServices_CreateNewInstancesPerScope()
        {
            using (var scope1 = _factory.Services.CreateScope())
            {
                using (var scope2 = _factory.Services.CreateScope())
                {
                    var service1 = scope1.ServiceProvider.GetRequiredService<ProjectDataService>();
                    var service2 = scope2.ServiceProvider.GetRequiredService<ProjectDataService>();

                    Assert.NotSame(service1, service2);
                }
            }
        }

        [Fact]
        public void DependencyInjection_PersistenceServices_AreConfigured()
        {
            var stateStore = _factory.Services.GetRequiredService<AgentStateStore>();
            var memoryStore = _factory.Services.GetRequiredService<AgentMemoryStore>();

            Assert.NotNull(stateStore);
            Assert.NotNull(memoryStore);
        }

        [Fact]
        public void DependencyInjection_ConfigurationOptions_AreBound()
        {
            using var scope = _factory.Services.CreateScope();
            var agentSquadOptions = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AgentSquadConfig>>();
            var limitsOptions = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<LimitsConfig>>();

            Assert.NotNull(agentSquadOptions.Value);
            Assert.NotNull(limitsOptions.Value);
        }
    }
}