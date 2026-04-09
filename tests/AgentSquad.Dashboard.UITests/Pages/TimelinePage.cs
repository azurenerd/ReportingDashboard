using Microsoft.Playwright;

namespace AgentSquad.Dashboard.UITests.Pages
{
    public class TimelinePage
    {
        private readonly IPage _page;

        public TimelinePage(IPage page)
        {
            _page = page;
        }

        public async Task<bool> IsMilestoneTimelineVisibleAsync()
        {
            var timeline = await _page.QuerySelectorAsync(".timeline");
            return timeline != null;
        }

        public async Task<int> GetMilestoneCountAsync()
        {
            var milestones = await _page.QuerySelectorAllAsync(".timeline-item");
            return milestones.Count;
        }

        public async Task<bool> HasNoMilestonesMessageAsync()
        {
            var message = await _page.QuerySelectorAsync("text=No milestones defined");
            return message != null;
        }

        public async Task<string?> GetFirstMilestoneNameAsync()
        {
            var milestones = await _page.QuerySelectorAllAsync(".timeline-content h6");
            if (milestones.Count > 0)
            {
                return await milestones[0].TextContentAsync();
            }
            return null;
        }

        public async Task<string?> GetMilestoneStatusAsync(int index)
        {
            var markers = await _page.QuerySelectorAllAsync(".timeline-marker");
            if (markers.Count > index)
            {
                var classAttribute = await markers[index].GetAttributeAsync("class");
                return classAttribute;
            }
            return null;
        }

        public async Task<string?> GetMilestoneCompletionPercentageAsync(int index)
        {
            var progressBars = await _page.QuerySelectorAllAsync(".progress-bar");
            if (progressBars.Count > index)
            {
                return await progressBars[index].GetAttributeAsync("style");
            }
            return null;
        }

        public async Task<int> GetCompletedMilestonesCountAsync()
        {
            var completedMarkers = await _page.QuerySelectorAllAsync(".timeline-marker.completed");
            return completedMarkers.Count;
        }

        public async Task<int> GetInProgressMilestonesCountAsync()
        {
            var inProgressMarkers = await _page.QuerySelectorAllAsync(".timeline-marker.in-progress");
            return inProgressMarkers.Count;
        }

        public async Task<int> GetPendingMilestonesCountAsync()
        {
            var pendingMarkers = await _page.QuerySelectorAllAsync(".timeline-marker.pending");
            return pendingMarkers.Count;
        }
    }
}