using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AgentSquad.Dashboard.Services;
using System;
using System.Collections.Generic;

namespace AgentSquad.Dashboard.Tests.Integration
{
    [Trait("Category", "Integration")]
    public class DependencyInjectionIntegrationTests
    {
        private readonly ServiceCollection _services;

        public DependencyInjectionIntegrationTests()
        {
            _services = new ServiceCollection();
            ConfigureServices(_services);
        }

        [Fact]
        public void ProjectDataService_CanBeResolved()
        {
            var serviceProvider = _services.BuildServiceProvider();
            var service = serviceProvider.GetRequiredService<ProjectDataService>();

            Assert.NotNull(service);
        }

        [Fact]
        public void ProjectDataService_IsRegisteredAsSingleton()
        {
            var serviceProvider = _services.BuildServiceProvider();
            var service1 = serviceProvider.GetRequiredService<ProjectDataService>();
            var service2 = serviceProvider.GetRequiredService<ProjectDataService>();

            Assert.Same(service1, service2);
        }

        [Fact]
        public void Logger_CanBeResolved()
        {
            var serviceProvider = _services.BuildServiceProvider();
            var logger = serviceProvider.GetRequiredService<ILogger<ProjectDataService>>();

            Assert.NotNull(logger);
        }

        [Fact]
        public void MultipleServices_CanBeResolved()
        {
            var serviceProvider = _services.BuildServiceProvider();
            var projectDataService = serviceProvider.GetRequiredService<ProjectDataService>();
            var logger = serviceProvider.GetRequiredService<ILogger<ProjectDataService>>();

            Assert.NotNull(projectDataService);
            Assert.NotNull(logger);
        }

        [Fact]
        public void ServiceProvider_DisposesCorrectly()
        {
            var serviceProvider = _services.BuildServiceProvider();
            var service = serviceProvider.GetRequiredService<ProjectDataService>();

            Assert.NotNull(service);
            serviceProvider.Dispose();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder => builder.AddDebug());
            services.AddSingleton<ProjectDataService>();
        }
    }
}