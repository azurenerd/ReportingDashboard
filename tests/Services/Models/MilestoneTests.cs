using Xunit;
using FluentAssertions;
using AgentSquad.Services.Models;

namespace AgentSquad.Tests.Services.Models
{
    public class MilestoneTests
    {
        [Fact]
        public void Milestone_CompletionPercentage_AcceptsValidValues()
        {
            var milestone = new Milestone { CompletionPercentage = 50 };
            
            milestone.CompletionPercentage.Should().Be(50);
        }

        [Fact]
        public void Milestone_CompletionPercentage_AcceptsZero()
        {
            var milestone = new Milestone { CompletionPercentage = 0 };
            
            milestone.CompletionPercentage.Should().Be(0);
        }

        [Fact]
        public void Milestone_CompletionPercentage_Accepts100()
        {
            var milestone = new Milestone { CompletionPercentage = 100 };
            
            milestone.CompletionPercentage.Should().Be(100);
        }

        [Fact]
        public void Milestone_CompletionPercentage_RejectsNegativeValues()
        {
            var milestone = new Milestone();
            
            milestone.Invoking(m => m.CompletionPercentage = -1)
                .Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Milestone_CompletionPercentage_RejectsValuesOver100()
        {
            var milestone = new Milestone();
            
            milestone.Invoking(m => m.CompletionPercentage = 101)
                .Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Milestone_Status_UsesEnumType()
        {
            var milestone = new Milestone { Status = MilestoneStatus.Active };
            
            milestone.Status.Should().Be(MilestoneStatus.Active);
        }
    }
}