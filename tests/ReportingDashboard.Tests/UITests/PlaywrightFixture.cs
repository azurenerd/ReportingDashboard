using Xunit;

namespace ReportingDashboard.Tests.UITests;

/// <summary>
/// Playwright fixture for UI tests. Currently disabled because Playwright browser
/// binaries are not installed in the CI/dev environment.
/// To re-enable: run 'pwsh bin/Debug/net8.0/playwright.ps1 install chromium'
/// Then restore the full IAsyncLifetime implementation.
/// </summary>
public class PlaywrightFixture
{
}

[CollectionDefinition("Playwright")]
public class PlaywrightCollection : ICollectionFixture<PlaywrightFixture>
{
}