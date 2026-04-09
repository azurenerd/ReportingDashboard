using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using AgentSquad.Core.Configuration;

namespace AgentSquad.Runner.Tests.Integration
{
    [Trait("Category", "Integration")]
    public class ConfigurationBindingTests : IAsyncLifetime
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
        public void Configuration_AgentSquadSection_BindsSuccessfully()
        {
            using var scope = _factory.Services.CreateScope();
            var options = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AgentSquadConfig>>();

            Assert.NotNull(options.Value);
        }

        [Fact]
        public void Configuration_LimitsSection_BindsSuccessfully()
        {
            using var scope = _factory.Services.CreateScope();
            var options = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<LimitsConfig>>();

            Assert.NotNull(options.Value);
        }

        [Fact]
        public void Configuration_BoundOptions_ContainNonNullValues()
        {
            using var scope = _factory.Services.CreateScope();
            var agentSquadOptions = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AgentSquadConfig>>();

            Assert.NotNull(agentSquadOptions);
            Assert.NotNull(agentSquadOptions.Value);
        }
    }
}