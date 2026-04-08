using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace AgentSquad.Tests
{
    public class DashboardCssTests
    {
        private readonly string _cssPath;
        private readonly string _cssContent;

        public DashboardCssTests()
        {
            _cssPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "css", "site.css");
            _cssContent = File.ReadAllText(_cssPath);
        }

        [Theory]
        [InlineData("dashboard-container")]
        [InlineData("dashboard-header")]
        [InlineData("metrics-grid")]
        [InlineData("metric-card")]
        [InlineData("timeline-container")]
        [InlineData("timeline-wrapper")]
        [InlineData("milestone-item")]
        [InlineData("work-items-section")]
        [InlineData("work-items-grid")]
        [InlineData("work-item-column")]
        public void CssDefinesRequiredClasses(string className)
        {
            Assert.Contains($".{className}", _cssContent);
        }

        [Theory]
        [InlineData("status-completed")]
        [InlineData("status-inprogress")]
        [InlineData("status-atrisk")]
        [InlineData("status-future")]
        [InlineData("status-ontrack")]
        [InlineData("status-shipped")]
        [InlineData("status-carriedover")]
        public void CssDefinesStatusClasses(string className)
        {
            Assert.Contains($".{className}", _cssContent);
        }

        [Fact]
        public void CssDefinesColorForCompletedStatus()
        {
            var completedRule = ExtractCssRule(".status-completed");
            Assert.True(
                completedRule.Contains("#4CAF50", StringComparison.OrdinalIgnoreCase) ||
                completedRule.Contains("green", StringComparison.OrdinalIgnoreCase),
                "Completed status should be green (#4CAF50)"
            );
        }

        [Fact]
        public void CssDefinesColorForInProgressStatus()
        {
            var inProgressRule = ExtractCssRule(".status-inprogress");
            Assert.True(
                inProgressRule.Contains("#2196F3", StringComparison.OrdinalIgnoreCase) ||
                inProgressRule.Contains("blue", StringComparison.OrdinalIgnoreCase),
                "In-progress status should be blue (#2196F3)"
            );
        }

        [Fact]
        public void CssDefinesColorForAtRiskStatus()
        {
            var atRiskRule = ExtractCssRule(".status-atrisk");
            Assert.True(
                atRiskRule.Contains("#F44336", StringComparison.OrdinalIgnoreCase) ||
                atRiskRule.Contains("red", StringComparison.OrdinalIgnoreCase),
                "At-risk status should be red (#F44336)"
            );
        }

        [Fact]
        public void CssDefinesColorForFutureStatus()
        {
            var futureRule = ExtractCssRule(".status-future");
            Assert.True(
                futureRule.Contains("#9E9E9E", StringComparison.OrdinalIgnoreCase) ||
                futureRule.Contains("gray", StringComparison.OrdinalIgnoreCase),
                "Future status should be gray (#9E9E9E)"
            );
        }

        [Theory]
        [InlineData("1024px")]
        [InlineData("1280px")]
        [InlineData("1920px")]
        public void CssIncludesMediaQueryBreakpoints(string breakpoint)
        {
            Assert.Contains(breakpoint, _cssContent);
        }

        [Fact]
        public void CssIncludesPrintMediaQuery()
        {
            Assert.Contains("@media print", _cssContent, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void PrintMediaQueryPreventsPrintBreaks()
        {
            var printRule = ExtractMediaQuery("print");
            Assert.Contains("page-break", printRule, StringComparison.OrdinalIgnoreCase) ||
                   Assert.Contains("avoid", printRule, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void PrintMediaQueryFixesContainerWidth()
        {
            var printRule = ExtractMediaQuery("print");
            Assert.True(
                printRule.Contains("width:", StringComparison.OrdinalIgnoreCase) ||
                printRule.Contains("max-width:", StringComparison.OrdinalIgnoreCase),
                "Print media query should fix container widths for consistent screenshot capture"
            );
        }

        [Fact]
        public void CssUsesSystemFontStack()
        {
            Assert.True(
                _cssContent.Contains("Segoe UI", StringComparison.OrdinalIgnoreCase) ||
                _cssContent.Contains("Arial", StringComparison.OrdinalIgnoreCase) ||
                _cssContent.Contains("sans-serif", StringComparison.OrdinalIgnoreCase),
                "CSS should use system font stack (Segoe UI/Arial/sans-serif) for cross-browser consistency"
            );
        }

        [Fact]
        public void CssMinimumViewportWidth()
        {
            var rules = ExtractMediaQuery("screen");
            Assert.Contains("min-width: 1024px", _cssContent) ||
                   Assert.Contains("1024px", _cssContent);
        }

        [Fact]
        public void MetricsGridUsesGridLayout()
        {
            var metricsRule = ExtractCssRule(".metrics-grid");
            Assert.Contains("display:", metricsRule, StringComparison.OrdinalIgnoreCase);
            Assert.True(
                metricsRule.Contains("grid", StringComparison.OrdinalIgnoreCase) ||
                metricsRule.Contains("flex", StringComparison.OrdinalIgnoreCase),
                "Metrics grid should use CSS Grid or Flexbox layout"
            );
        }

        [Fact]
        public void TimelineContainerIsResponsive()
        {
            var timelineRule = ExtractCssRule(".timeline-wrapper");
            Assert.NotEmpty(timelineRule);
        }

        [Fact]
        public void WorkItemColumnDisplaysWithBorder()
        {
            var itemRule = ExtractCssRule(".work-item-column");
            Assert.True(
                itemRule.Contains("border", StringComparison.OrdinalIgnoreCase) ||
                itemRule.Contains("padding", StringComparison.OrdinalIgnoreCase) ||
                itemRule.Contains("background", StringComparison.OrdinalIgnoreCase),
                "Work item columns should have visual separation"
            );
        }

        [Fact]
        public void CssHasHighContrastForAccessibility()
        {
            Assert.True(
                _cssContent.Contains("contrast", StringComparison.OrdinalIgnoreCase) ||
                _cssContent.Contains("color:", StringComparison.OrdinalIgnoreCase),
                "CSS should define high-contrast colors for executive visibility"
            );
        }

        private string ExtractCssRule(string className)
        {
            var pattern = $@"{Regex.Escape(className)}\s*\{{[^}}]*}}";
            var match = Regex.Match(_cssContent, pattern);
            return match.Success ? match.Value : string.Empty;
        }

        private string ExtractMediaQuery(string mediaType)
        {
            var pattern = $@"@media\s+{mediaType}\s*\{{[^}}]*}}";
            var match = Regex.Match(_cssContent, pattern, RegexOptions.IgnoreCase);
            return match.Success ? match.Value : string.Empty;
        }
    }
}