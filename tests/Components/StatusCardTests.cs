using System;
using Xunit;
using Bunit;
using AgentSquad.Components;

namespace AgentSquad.Tests.Components
{
    public class StatusCardTests : TestContext
    {
        [Fact]
        public void StatusCard_ThrowsWhenStatusCategoryNotProvided()
        {
            var parameters = new ComponentParameter[] { };
            
            var exception = Assert.Throws<ComponentRenderException>(() =>
            {
                RenderComponent<StatusCard>(parameters);
            });
            
            Assert.NotNull(exception);
        }
        
        [Fact]
        public void StatusCard_RendersWithValidStatus()
        {
            var parameters = new ComponentParameter[]
            {
                ComponentParameter.CreateParameter("StatusCategory", "Active")
            };
            
            var component = RenderComponent<StatusCard>(parameters);
            
            Assert.Contains("Active", component.Markup);
        }
    }
}