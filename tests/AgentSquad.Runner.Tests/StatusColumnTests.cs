using Bunit;
using Xunit;
using AgentSquad.Runner.Components;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Tests
{
    public class StatusColumnTests : TestContext
    {
        [Fact]
        public void StatusColumn_DisplaysEmptyStateWithNullItems()
        {
            var component = RenderComponent<StatusColumn>(parameters =>
                parameters
                    .Add(p => p.Title, "Shipped")
                    .Add(p => p.Items, null)
                    .Add(p => p.BadgeClass, "success")
            );

            component.WaitForAssertion(() =>
            {
                var markup = component.Markup;
                Assert.Contains("No items", markup);
            });
        }

        [Fact]
        public void StatusColumn_DisplaysEmptyStateWithEmptyList()
        {
            var component = RenderComponent<StatusColumn>(parameters =>
                parameters
                    .Add(p => p.Title, "Shipped")
                    .Add(p => p.Items, new List<WorkItem>())
                    .Add(p => p.BadgeClass, "success")
            );

            component.WaitForAssertion(() =>
            {
                var markup = component.Markup;
                Assert.Contains("No items", markup);
            });
        }

        [Fact]
        public void StatusColumn_DisplaysCountBadge()
        {
            var items = new List<WorkItem>
            {
                new WorkItem { Id = "w1", Title = "Item 1", CompletedDate = DateTime.Now },
                new WorkItem { Id = "w2", Title = "Item 2", CompletedDate = DateTime.Now },
                new WorkItem { Id = "w3", Title = "Item 3", CompletedDate = DateTime.Now },
                new WorkItem { Id = "w4", Title = "Item 4", CompletedDate = DateTime.Now },
                new WorkItem { Id = "w5", Title = "Item 5", CompletedDate = DateTime.Now }
            };

            var component = RenderComponent<StatusColumn>(parameters =>
                parameters
                    .Add(p => p.Title, "Shipped")
                    .Add(p => p.Items, items)
                    .Add(p => p.BadgeClass, "success")
            );

            component.WaitForAssertion(() =>
            {
                var markup = component.Markup;
                Assert.Contains(">5<", markup);
            });
        }

        [Fact]
        public void StatusColumn_RenderAllItems()
        {
            var items = new List<WorkItem>
            {
                new WorkItem { Id = "w1", Title = "Item 1", CompletedDate = DateTime.Now },
                new WorkItem { Id = "w2", Title = "Item 2", CompletedDate = DateTime.Now },
                new WorkItem { Id = "w3", Title = "Item 3", CompletedDate = DateTime.Now },
                new WorkItem { Id = "w4", Title = "Item 4", CompletedDate = DateTime.Now },
                new WorkItem { Id = "w5", Title = "Item 5", CompletedDate = DateTime.Now }
            };

            var component = RenderComponent<StatusColumn>(parameters =>
                parameters
                    .Add(p => p.Title, "Shipped")
                    .Add(p => p.Items, items)
                    .Add(p => p.BadgeClass, "success")
            );

            component.WaitForAssertion(() =>
            {
                var markup = component.Markup;
                foreach (var item in items)
                {
                    Assert.Contains(item.Title, markup);
                }
            });
        }

        [Fact]
        public void StatusColumn_SortsItemsReverseChronologically()
        {
            var baseDate = new DateTime(2026, 1, 1);
            var items = new List<WorkItem>
            {
                new WorkItem { Id = "w1", Title = "Oldest Item", CompletedDate = baseDate },
                new WorkItem { Id = "w2", Title = "Middle Item", CompletedDate = baseDate.AddDays(5) },
                new WorkItem { Id = "w3", Title = "Newest Item", CompletedDate = baseDate.AddDays(10) }
            };

            var component = RenderComponent<StatusColumn>(parameters =>
                parameters
                    .Add(p => p.Title, "Shipped")
                    .Add(p => p.Items, items)
                    .Add(p => p.BadgeClass, "success")
            );

            component.WaitForAssertion(() =>
            {
                var markup = component.Markup;
                var newestIndex = markup.IndexOf("Newest Item");
                var middleIndex = markup.IndexOf("Middle Item");
                var oldestIndex = markup.IndexOf("Oldest Item");

                Assert.True(newestIndex < middleIndex);
                Assert.True(middleIndex < oldestIndex);
            });
        }

        [Fact]
        public void StatusColumn_FormatsDateCorrectly()
        {
            var date = new DateTime(2026, 2, 28);
            var items = new List<WorkItem>
            {
                new WorkItem { Id = "w1", Title = "Test Item", CompletedDate = date }
            };

            var component = RenderComponent<StatusColumn>(parameters =>
                parameters
                    .Add(p => p.Title, "Shipped")
                    .Add(p => p.Items, items)
                    .Add(p => p.BadgeClass, "success")
            );

            component.WaitForAssertion(() =>
            {
                var markup = component.Markup;
                Assert.Contains("Feb 28, 2026", markup);
            });
        }

        [Fact]
        public void StatusColumn_IncludesDescriptionWhenProvided()
        {
            var items = new List<WorkItem>
            {
                new WorkItem
                {
                    Id = "w1",
                    Title = "Item with Description",
                    Description = "This is a test description",
                    CompletedDate = DateTime.Now
                }
            };

            var component = RenderComponent<StatusColumn>(parameters =>
                parameters
                    .Add(p => p.Title, "Shipped")
                    .Add(p => p.Items, items)
                    .Add(p => p.BadgeClass, "success")
            );

            component.WaitForAssertion(() =>
            {
                var markup = component.Markup;
                Assert.Contains("This is a test description", markup);
            });
        }

        [Fact]
        public void StatusColumn_OmitsDescriptionWhenNotProvided()
        {
            var items = new List<WorkItem>
            {
                new WorkItem
                {
                    Id = "w1",
                    Title = "Item without Description",
                    Description = null,
                    CompletedDate = DateTime.Now
                }
            };

            var component = RenderComponent<StatusColumn>(parameters =>
                parameters
                    .Add(p => p.Title, "Shipped")
                    .Add(p => p.Items, items)
                    .Add(p => p.BadgeClass, "success")
            );

            component.WaitForAssertion(() =>
            {
                var markup = component.Markup;
                Assert.DoesNotContain("<p class=\"work-item-description\"></p>", markup);
            });
        }

        [Fact]
        public void StatusColumn_AppliesBadgeClassSuccess()
        {
            var items = new List<WorkItem>
            {
                new WorkItem { Id = "w1", Title = "Item 1", CompletedDate = DateTime.Now }
            };

            var component = RenderComponent<StatusColumn>(parameters =>
                parameters
                    .Add(p => p.Title, "Shipped")
                    .Add(p => p.Items, items)
                    .Add(p => p.BadgeClass, "success")
            );

            component.WaitForAssertion(() =>
            {
                var markup = component.Markup;
                Assert.Contains("bg-success", markup);
            });
        }

        [Fact]
        public void StatusColumn_AppliesBadgeClassPrimary()
        {
            var items = new List<WorkItem>
            {
                new WorkItem { Id = "w1", Title = "Item 1", CompletedDate = DateTime.Now }
            };

            var component = RenderComponent<StatusColumn>(parameters =>
                parameters
                    .Add(p => p.Title, "In Progress")
                    .Add(p => p.Items, items)
                    .Add(p => p.BadgeClass, "primary")
            );

            component.WaitForAssertion(() =>
            {
                var markup = component.Markup;
                Assert.Contains("bg-primary", markup);
            });
        }

        [Fact]
        public void StatusColumn_AppliesBadgeClassWarning()
        {
            var items = new List<WorkItem>
            {
                new WorkItem { Id = "w1", Title = "Item 1", CompletedDate = DateTime.Now }
            };

            var component = RenderComponent<StatusColumn>(parameters =>
                parameters
                    .Add(p => p.Title, "Carried Over")
                    .Add(p => p.Items, items)
                    .Add(p => p.BadgeClass, "warning")
            );

            component.WaitForAssertion(() =>
            {
                var markup = component.Markup;
                Assert.Contains("bg-warning", markup);
            });
        }

        [Fact]
        public void StatusColumn_DisplaysColumnTitle()
        {
            var items = new List<WorkItem>
            {
                new WorkItem { Id = "w1", Title = "Item 1", CompletedDate = DateTime.Now }
            };

            var component = RenderComponent<StatusColumn>(parameters =>
                parameters
                    .Add(p => p.Title, "Shipped")
                    .Add(p => p.Items, items)
                    .Add(p => p.BadgeClass, "success")
            );

            component.WaitForAssertion(() =>
            {
                var markup = component.Markup;
                Assert.Contains("Shipped", markup);
            });
        }
    }
}