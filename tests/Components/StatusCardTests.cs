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
            Assert.Throws<ComponentRenderException>(() =>
            {
                RenderComponent<StatusCard>();
            });
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
        
        [Fact]
        public void StatusCard_DisplaysCorrectStatusValue()
        {
            var parameters = new ComponentParameter[]
            {
                ComponentParameter.CreateParameter("StatusCategory", "Completed")
            };
            
            var component = RenderComponent<StatusCard>(parameters);
            
            Assert.Contains("Completed", component.Markup);
        }
    }
}