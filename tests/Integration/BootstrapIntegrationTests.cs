using Xunit;
using Bunit;
using AgentSquad.Components;

namespace AgentSquad.Tests.Integration
{
    public class BootstrapIntegrationTests : TestContext
    {
        [Fact]
        public void BootstrapGrid_CorrectlyAppliesResponsiveClasses()
        {
            var component = RenderComponent<Dashboard>();
            
            var responsiveRows = component.FindAll(".row");
            Assert.NotEmpty(responsiveRows);
            
            foreach (var row in responsiveRows)
            {
                var colElements = row.QuerySelectorAll("[class*='col-']");
                foreach (var col in colElements)
                {
                    Assert.True(col.ClassList.Contains("col-sm-12") || 
                               col.ClassList.Contains("col-md-6") ||
                               col.ClassList.Contains("col-lg-4"));
                }
            }
        }
        
        [Fact]
        public void BootstrapGrid_MaintainsProperHierarchy()
        {
            var component = RenderComponent<Dashboard>();
            var rows = component.FindAll(".row");
            
            foreach (var row in rows)
            {
                var parent = row.ParentElement;
                Assert.NotNull(parent);
                Assert.True(parent.ClassList.Contains("container") || 
                           parent.ClassList.Contains("container-fluid"));
            }
        }
    }
}