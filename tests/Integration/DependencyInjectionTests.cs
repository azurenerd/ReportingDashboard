using Xunit;
using Microsoft.Extensions.DependencyInjection;
using AgentSquad.Runner.Services;

namespace AgentSquad.Runner.Tests.Integration
{
    public class DependencyInjectionTests
    {
        [Fact]
        public void ServiceCollection_WithProjectDataServiceSingletonRegistration_ResolvesSameInstance()
        {
            var services = new ServiceCollection();
            var mockEnvironment = new MockWebHostEnvironment(Path.GetTempPath());
            
            services.AddSingleton<IWebHostEnvironment>(mockEnvironment);
            services.AddSingleton<ProjectDataService>();

            var provider = services.BuildServiceProvider();
            var instance1 = provider.GetRequiredService<ProjectDataService>();
            var instance2 = provider.GetRequiredService<ProjectDataService>();

            Assert.Same(instance1, instance2);
        }

        [Fact]
        public void ServiceCollection_WithProjectDataServiceRegistration_CanResolveService()
        {
            var services = new ServiceCollection();
            var mockEnvironment = new MockWebHostEnvironment(Path.GetTempPath());
            
            services.AddSingleton<IWebHostEnvironment>(mockEnvironment);
            services.AddSingleton<ProjectDataService>();

            var provider = services.BuildServiceProvider();
            var service = provider.GetRequiredService<ProjectDataService>();

            Assert.NotNull(service);
            Assert.IsType<ProjectDataService>(service);
        }

        [Fact]
        public void ServiceCollection_WithoutProjectDataServiceRegistration_ThrowsInvalidOperationException()
        {
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();

            Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<ProjectDataService>());
        }

        [Fact]
        public void ServiceCollection_WithMultipleScopes_EachScopeResolvesFromSingleton()
        {
            var services = new ServiceCollection();
            var mockEnvironment = new MockWebHostEnvironment(Path.GetTempPath());
            
            services.AddSingleton<IWebHostEnvironment>(mockEnvironment);
            services.AddSingleton<ProjectDataService>();

            var provider = services.BuildServiceProvider();
            
            ProjectDataService service1, service2, service3;
            using (var scope1 = provider.CreateScope())
            {
                service1 = scope1.ServiceProvider.GetRequiredService<ProjectDataService>();
            }
            using (var scope2 = provider.CreateScope())
            {
                service2 = scope2.ServiceProvider.GetRequiredService<ProjectDataService>();
            }
            using (var scope3 = provider.CreateScope())
            {
                service3 = scope3.ServiceProvider.GetRequiredService<ProjectDataService>();
            }

            Assert.Same(service1, service2);
            Assert.Same(service2, service3);
        }

        private class MockWebHostEnvironment : IWebHostEnvironment
        {
            private readonly string _webRootPath;

            public MockWebHostEnvironment(string webRootPath)
            {
                _webRootPath = webRootPath;
            }

            public string EnvironmentName { get; set; } = "Development";
            public string ApplicationName { get; set; } = "AgentSquad.Runner";
            public string WebRootPath => _webRootPath;
            public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
            public IFileProvider WebRootFileProvider { get; set; }
            public IFileProvider ContentRootFileProvider { get; set; }
        }
    }
}