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
            
            var mobileColumns = component.FindAll(".col-12");
            Assert.NotEmpty(mobileColumns);
            
            foreach (var col in mobileColumns)
            {
                Assert.True(col.ClassList.Contains("col-12"));
                Assert.NotNull(col.ParentElement);
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
        
        [Fact]
        public void AC3_BootstrapGridHierarchyCorrect()
        {
            var component = RenderComponent<Dashboard>();
            
            var containers = component.FindAll(".container, .container-fluid");
            Assert.NotEmpty(containers);
            
            var rows = component.FindAll(".row");
            Assert.NotEmpty(rows);
            
            foreach (var row in rows)
            {
                var columns = row.QuerySelectorAll("[class*='col-']");
                Assert.NotEmpty(columns);
            }
        }
    }
}