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
    }
}