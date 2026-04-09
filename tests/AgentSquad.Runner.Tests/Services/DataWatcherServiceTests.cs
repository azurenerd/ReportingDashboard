using System;
using System.IO;
using System.Threading.Tasks;
using AgentSquad.Runner.Interfaces;
using AgentSquad.Runner.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace AgentSquad.Runner.Tests.Services
{
    public class DataWatcherServiceTests : IDisposable
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<DataWatcherService>> _mockLogger;
        private readonly string _tempDirectory;
        private readonly string _tempDataFile;

        public DataWatcherServiceTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<DataWatcherService>>();
            
            _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDirectory);
            _tempDataFile = Path.Combine(_tempDirectory, "data.json");
            File.WriteAllText(_tempDataFile, "{}");

            _mockConfiguration
                .Setup(x => x.GetValue(It.IsAny<string>(), It.IsAny<int>()))
                .Returns<string, int>((key, defaultValue) =>
                {
                    if (key == "AppSettings:DebounceIntervalMs")
                        return 500;
                    return defaultValue;
                });

            _mockConfiguration
                .Setup(x => x.GetValue<string>(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((key, defaultValue) =>
                {
                    if (key == "AppSettings:DataPath")
                        return null;
                    return defaultValue;
                });
        }

        [Fact]
        public void Start_CreatesFileSystemWatcher()
        {
            var service = new DataWatcherService(_mockConfiguration.Object, _mockLogger.Object);
            service.Start(_tempDataFile);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("DataWatcher started successfully")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            service.Dispose();
        }

        [Fact]
        public async Task OnFileChanged_DebouncesSingleRefresh()
        {
            var service = new DataWatcherService(_mockConfiguration.Object, _mockLogger.Object);
            var eventFiredCount = 0;
            var taskCompletionSource = new TaskCompletionSource<bool>();

            service.OnDataChanged += async () =>
            {
                eventFiredCount++;
                if (eventFiredCount == 1)
                    taskCompletionSource.SetResult(true);
                await Task.CompletedTask;
            };

            service.Start(_tempDataFile);
            var initialRefreshTime = service.LastRefreshTime;

            for (int i = 0; i < 5; i++)
            {
                File.WriteAllText(_tempDataFile, $"{{\"update\": {i}}}");
                await Task.Delay(20);
            }

            var completed = await taskCompletionSource.Task.ConfigureAwait(false);

            completed.Should().BeTrue();
            eventFiredCount.Should().Be(1, "debounce should collapse multiple writes into single event");
            service.LastRefreshTime.Should().BeGreaterThan(initialRefreshTime);

            service.Dispose();
        }

        [Fact]
        public async Task OnFileChanged_MultipleSeparateWrites_FiresMultipleEvents()
        {
            var service = new DataWatcherService(_mockConfiguration.Object, _mockLogger.Object);
            var eventFiredCount = 0;
            var firstTaskCompletionSource = new TaskCompletionSource<bool>();
            var secondTaskCompletionSource = new TaskCompletionSource<bool>();

            service.OnDataChanged += async () =>
            {
                eventFiredCount++;
                if (eventFiredCount == 1)
                    firstTaskCompletionSource.SetResult(true);
                else if (eventFiredCount == 2)
                    secondTaskCompletionSource.SetResult(true);
                await Task.CompletedTask;
            };

            service.Start(_tempDataFile);

            File.WriteAllText(_tempDataFile, "{\"update\": 1}");
            var firstCompleted = await firstTaskCompletionSource.Task.ConfigureAwait(false);

            await Task.Delay(600);

            File.WriteAllText(_tempDataFile, "{\"update\": 2}");
            var secondCompleted = await secondTaskCompletionSource.Task.ConfigureAwait(false);

            firstCompleted.Should().BeTrue();
            secondCompleted.Should().BeTrue();
            eventFiredCount.Should().Be(2, "debounce should reset between separate write windows");

            service.Dispose();
        }

        [Fact]
        public async Task LastRefreshTime_UpdatesOnDebounceComplete()
        {
            var service = new DataWatcherService(_mockConfiguration.Object, _mockLogger.Object);
            var taskCompletionSource = new TaskCompletionSource<bool>();

            service.OnDataChanged += async () =>
            {
                taskCompletionSource.SetResult(true);
                await Task.CompletedTask;
            };

            service.Start(_tempDataFile);
            var initialRefreshTime = service.LastRefreshTime;
            await Task.Delay(100);

            File.WriteAllText(_tempDataFile, "{\"update\": 1}");
            await taskCompletionSource.Task.ConfigureAwait(false);

            service.LastRefreshTime.Should().BeGreaterThan(initialRefreshTime);

            service.Dispose();
        }

        [Fact]
        public void LastRefreshTimeFormatted_ReturnsHHmmssFormat()
        {
            var service = new DataWatcherService(_mockConfiguration.Object, _mockLogger.Object);
            service.Start(_tempDataFile);

            var formatted = service.LastRefreshTimeFormatted;

            formatted.Should().MatchRegex(@"^\d{2}:\d{2}:\d{2}$");

            service.Dispose();
        }

        [Fact]
        public async Task Stop_DisablesFileSystemWatcher()
        {
            var service = new DataWatcherService(_mockConfiguration.Object, _mockLogger.Object);
            var eventFiredCount = 0;
            var taskCompletionSource = new TaskCompletionSource<bool>();

            service.OnDataChanged += async () =>
            {
                eventFiredCount++;
                taskCompletionSource.SetResult(true);
                await Task.CompletedTask;
            };

            service.Start(_tempDataFile);

            File.WriteAllText(_tempDataFile, "{\"update\": 1}");
            await taskCompletionSource.Task.ConfigureAwait(false);
            int countBeforeStop = eventFiredCount;

            service.Stop();
            await Task.Delay(100);

            File.WriteAllText(_tempDataFile, "{\"update\": 2}");
            await Task.Delay(600);

            countBeforeStop.Should().Be(1);
            eventFiredCount.Should().Be(1, "after Stop(), file changes should not trigger events");

            service.Dispose();
        }

        [Fact]
        public void Start_WithInvalidDirectory_LogsWarningAndContinues()
        {
            var invalidPath = Path.Combine(_tempDirectory, "nonexistent", "data.json");
            var service = new DataWatcherService(_mockConfiguration.Object, _mockLogger.Object);

            service.Start(invalidPath);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to start DataWatcher")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            service.Dispose();
        }

        [Fact]
        public void Dispose_CleansUpResources()
        {
            var service = new DataWatcherService(_mockConfiguration.Object, _mockLogger.Object);
            service.Start(_tempDataFile);

            service.Dispose();

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Stopping DataWatcher")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DisposeAsync_CleansUpResourcesAsync()
        {
            var service = new DataWatcherService(_mockConfiguration.Object, _mockLogger.Object);
            service.Start(_tempDataFile);

            await service.DisposeAsync();

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Stopping DataWatcher")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task IntegrationSmoke_FileWatcherActivation()
        {
            var service = new DataWatcherService(_mockConfiguration.Object, _mockLogger.Object);
            var eventFired = false;
            var taskCompletionSource = new TaskCompletionSource<bool>();

            service.OnDataChanged += async () =>
            {
                eventFired = true;
                taskCompletionSource.SetResult(true);
                await Task.CompletedTask;
            };

            service.Start(_tempDataFile);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("DataWatcher started successfully")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            File.WriteAllText(_tempDataFile, "{\"test\": true}");
            var completed = await taskCompletionSource.Task.ConfigureAwait(false);

            completed.Should().BeTrue();
            eventFired.Should().BeTrue();

            service.Stop();
            await Task.Delay(100);

            File.WriteAllText(_tempDataFile, "{\"test\": false}");
            await Task.Delay(600);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Stopping DataWatcher")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            service.Dispose();
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDirectory))
                Directory.Delete(_tempDirectory, true);
        }
    }
}