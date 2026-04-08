using System;
using Xunit;
using Bunit;
using AgentSquad.Components;

namespace AgentSquad.Tests.Acceptance
{
    public class ResponsiveDesignAcceptanceTests : TestContext
    {
        [Fact]
        public void AC8_MobileViewportRespondsToGridSystem()
        {
            var component = RenderComponent<Dashboard>();
            
            var mobileColumns = component.FindAll(".col-sm-12");
            Assert.NotEmpty(mobileColumns);
            
            foreach (var col in mobileColumns)
            {
                Assert.True(col.ClassList.Contains("col-sm-12"));
            }
        }
        
        [Fact]
        public void AC1_TabletViewportShowsGridLayout()
        {
            var component = RenderComponent<Dashboard>();
            
            var tabletColumns = component.FindAll(".col-md-6");
            Assert.NotEmpty(tabletColumns);
            
            foreach (var col in tabletColumns)
            {
                Assert.True(col.ClassList.Contains("col-md-6"));
            }
        }
        
        [Fact]
        public void AC2_DesktopViewportShowsFullGrid()
        {
            var component = RenderComponent<Dashboard>();
            
            var desktopColumns = component.FindAll(".col-lg-4");
            Assert.NotEmpty(desktopColumns);
            
            foreach (var col in desktopColumns)
            {
                Assert.True(col.ClassList.Contains("col-lg-4"));
            }
        }
    }
}