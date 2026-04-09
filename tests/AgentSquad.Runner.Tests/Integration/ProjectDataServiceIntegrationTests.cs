using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using AgentSquad.Dashboard.Services;
using Moq;

namespace AgentSquad.Runner.Tests.Integration
{
    [Trait("Category", "Integration")]
    public class ProjectDataServiceIntegrationTests : IAsyncLifetime
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
        public void ProjectDataService_CanBeResolved_FromDIContainer()
        {
            using var scope = _factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ProjectDataService>();

            Assert.NotNull(service);
        }

        [Fact]
        public void ProjectDataService_WhenResolvedInMultipleScopes_CreatesDifferentInstances()
        {
            using var scope1 = _factory.Services.CreateScope();
            using var scope2 = _factory.Services.CreateScope();

            var service1 = scope1.ServiceProvider.GetRequiredService<ProjectDataService>();
            var service2 = scope2.ServiceProvider.GetRequiredService<ProjectDataService>();

            Assert.NotSame(service1, service2);
        }

        [Fact]
        public void ProjectDataService_HasAccessToConfigurationService()
        {
            using var scope = _factory.Services.CreateScope();
            var configService = scope.ServiceProvider.GetRequiredService<ConfigurationService>();
            var projectDataService = scope.ServiceProvider.GetRequiredService<ProjectDataService>();

            Assert.NotNull(configService);
            Assert.NotNull(projectDataService);
        }
    }
}