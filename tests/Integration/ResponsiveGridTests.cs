using Xunit;
using Bunit;
using AngleSharp.Dom;
using AgentSquad.Components;

namespace AgentSquad.Tests.Integration
{
    public class ResponsiveGridTests : TestContext
    {
        [Fact]
        public void ResponsiveGrid_AppliesBootstrapClassesToCorrectElements()
        {
            var component = RenderComponent<Dashboard>();
            
            var mdColumns = component.FindAll(".col-md-6");
            Assert.NotEmpty(mdColumns);
            
            foreach (var element in mdColumns)
            {
                Assert.NotNull(element);
                Assert.True(element.ClassList.Contains("col-md-6"));
            }
        }
        
        [Fact]
        public void ResponsiveGrid_AppliesLargeScreenClasses()
        {
            var component = RenderComponent<Dashboard>();
            
            var lgColumns = component.FindAll(".col-lg-4");
            Assert.NotEmpty(lgColumns);
            
            foreach (var element in lgColumns)
            {
                Assert.True(element.ClassList.Contains("col-lg-4"));
            }
        }
        
        [Fact]
        public void ResponsiveGrid_RowsHaveContainerParent()
        {
            var component = RenderComponent<Dashboard>();
            var rows = component.FindAll(".row");
            
            Assert.NotEmpty(rows);
            foreach (var row in rows)
            {
                var parent = row.ParentElement;
                if (parent != null)
                {
                    Assert.True(parent.ClassList.Contains("container") || 
                               parent.ClassList.Contains("container-fluid"));
                }
            }
        }
    }
}