using Xunit;
using AgentSquad.Core.Persistence;
using System;

namespace AgentSquad.Runner.Tests.Unit
{
    [Trait("Category", "Unit")]
    public class AgentStateStoreTests
    {
        [Fact]
        public void Constructor_WithValidDbPath_InitializesSuccessfully()
        {
            var dbPath = "test_store.db";
            var store = new AgentStateStore(dbPath);

            Assert.NotNull(store);
        }

        [Fact]
        public void Constructor_WithEmptyPath_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => new AgentStateStore(""));
        }

        [Fact]
        public void Constructor_WithNullPath_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new AgentStateStore(null));
        }

        [Theory]
        [InlineData("valid_store.db")]
        [InlineData("agent_state.db")]
        [InlineData("production.db")]
        public void Constructor_WithVariousPaths_CreatesInstance(string dbPath)
        {
            var store = new AgentStateStore(dbPath);

            Assert.NotNull(store);
        }
    }
}