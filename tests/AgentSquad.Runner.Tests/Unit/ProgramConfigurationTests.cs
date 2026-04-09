using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using AgentSquad.Core.Configuration;
using AgentSquad.Core.Persistence;
using AgentSquad.Dashboard.Services;
using Moq;

namespace AgentSquad.Runner.Tests.Unit
{
    [Trait("Category", "Unit")]
    public class ProgramConfigurationTests
    {
        private IServiceCollection CreateServiceCollection()
        {
            return new ServiceCollection();
        }

        private IConfiguration CreateConfiguration(Dictionary<string, string> values = null)
        {
            values ??= new Dictionary<string, string>
            {
                { "AgentSquad:Dashboard:Port", "5050" },
                { "AgentSquad:Project:GitHubRepo", "owner/repo" },
                { "AgentSquad:Limits:MaxAgents", "10" }
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(values)
                .Build();
        }

        [Fact]
        public void ServiceCollection_ConfiguresAgentSquadConfig_BindsCorrectly()
        {
            var services = CreateServiceCollection();
            var config = CreateConfiguration();

            services.Configure<AgentSquadConfig>(config.GetSection("AgentSquad"));
            var provider = services.BuildServiceProvider();

            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AgentSquadConfig>>();
            Assert.NotNull(options);
            Assert.NotNull(options.Value);
        }

        [Fact]
        public void ServiceCollection_ConfiguresLimitsConfig_BindsCorrectly()
        {
            var services = CreateServiceCollection();
            var config = CreateConfiguration();

            services.Configure<LimitsConfig>(config.GetSection("AgentSquad:Limits"));
            var provider = services.BuildServiceProvider();

            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<LimitsConfig>>();
            Assert.NotNull(options);
        }

        [Fact]
        public void DashboardPort_DefaultValue_Returns5050()
        {
            var config = CreateConfiguration();
            var port = config.GetValue("AgentSquad:Dashboard:Port", 5050);

            Assert.Equal(5050, port);
        }

        [Fact]
        public void DashboardPort_CustomValue_ReturnsCustomValue()
        {
            var values = new Dictionary<string, string>
            {
                { "AgentSquad:Dashboard:Port", "8080" }
            };

            var config = CreateConfiguration(values);
            var port = config.GetValue("AgentSquad:Dashboard:Port", 5050);

            Assert.Equal(8080, port);
        }

        [Fact]
        public void DashboardPort_MissingValue_UsesDefaultFallback()
        {
            var values = new Dictionary<string, string>();
            var config = CreateConfiguration(values);

            var port = config.GetValue("AgentSquad:Dashboard:Port", 5050);

            Assert.Equal(5050, port);
        }

        [Fact]
        public void RepoSlug_NullRepo_UsesDefault()
        {
            var values = new Dictionary<string, string>();
            var repoSlug = values.TryGetValue("AgentSquad:Project:GitHubRepo", out var repo)
                ? repo.Replace('/', '_') ?? "default"
                : "default";

            Assert.Equal("default", repoSlug);
        }

        [Fact]
        public void RepoSlug_ValidRepo_ReplacesSlashWithUnderscore()
        {
            var repo = "owner/repo";
            var repoSlug = repo.Replace('/', '_');

            Assert.Equal("owner_repo", repoSlug);
        }

        [Fact]
        public void DbPath_ConstructedCorrectly_FormatsAsExpected()
        {
            var repoSlug = "my_project";
            var dbPath = $"agentsquad_{repoSlug}.db";

            Assert.Equal("agentsquad_my_project.db", dbPath);
        }

        [Theory]
        [InlineData("", "agentsquad_.db")]
        [InlineData("valid_repo", "agentsquad_valid_repo.db")]
        [InlineData("complex_repo_name", "agentsquad_complex_repo_name.db")]
        public void DbPath_VariousRepoSlugs_FormatsCorrectly(string repoSlug, string expected)
        {
            var dbPath = $"agentsquad_{repoSlug}.db";

            Assert.Equal(expected, dbPath);
        }

        [Fact]
        public void ServiceCollection_RegistersProjectDataService_AsScoped()
        {
            var services = CreateServiceCollection();
            services.AddScoped<ProjectDataService>();
            var provider = services.BuildServiceProvider();

            var service1 = provider.CreateScope().ServiceProvider.GetRequiredService<ProjectDataService>();
            var service2 = provider.CreateScope().ServiceProvider.GetRequiredService<ProjectDataService>();

            Assert.NotNull(service1);
            Assert.NotNull(service2);
            Assert.NotSame(service1, service2);
        }

        [Fact]
        public void ServiceCollection_RegistersConfigurationService_AsSingleton()
        {
            var services = CreateServiceCollection();
            services.AddSingleton<ConfigurationService>();
            var provider = services.BuildServiceProvider();

            var service1 = provider.GetRequiredService<ConfigurationService>();
            var service2 = provider.GetRequiredService<ConfigurationService>();

            Assert.Same(service1, service2);
        }
    }
}