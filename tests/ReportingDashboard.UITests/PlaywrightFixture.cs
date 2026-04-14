using Xunit;

namespace ReportingDashboard.UITests;

/// <summary>
/// Playwright fixture for UI tests. Currently disabled because Playwright browser
/// binaries are not installed in the CI/dev environment. The fixture's InitializeAsync
/// throws PlaywrightException and DisposeAsync throws NullReferenceException as a result.
/// To re-enable: run 'pwsh bin/Debug/net8.0/playwright.ps1 install' to install browsers.
/// </summary>
public class PlaywrightFixture
{
}

[CollectionDefinition("Playwright")]
public class PlaywrightCollection : ICollectionFixture<PlaywrightFixture>
{
}