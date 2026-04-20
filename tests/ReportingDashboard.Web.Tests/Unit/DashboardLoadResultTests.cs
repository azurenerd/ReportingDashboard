using ReportingDashboard.Web.Services;

namespace ReportingDashboard.Web.Tests.Unit;

public class DashboardLoadResultTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void DashboardLoadError_ConstructorAssignsAllFields()
    {
        var err = new DashboardLoadError("path.json", "boom", 3, 7, "NotFound");

        err.FilePath.Should().Be("path.json");
        err.Message.Should().Be("boom");
        err.Line.Should().Be(3);
        err.Column.Should().Be(7);
        err.Kind.Should().Be("NotFound");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void DashboardLoadError_AllowsNullLineAndColumn()
    {
        var err = new DashboardLoadError("p", "m", null, null, "ParseError");

        err.Line.Should().BeNull();
        err.Column.Should().BeNull();
        err.Kind.Should().Be("ParseError");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void DashboardLoadResult_AllowsNullDataWithError()
    {
        var err = new DashboardLoadError("p", "m", null, null, "ValidationError");
        var loadedAt = DateTimeOffset.UtcNow;

        var result = new DashboardLoadResult(null, err, loadedAt);

        result.Data.Should().BeNull();
        result.Error.Should().BeSameAs(err);
        result.LoadedAt.Should().Be(loadedAt);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void DashboardLoadResult_RecordEquality_WorksByValue()
    {
        var err = new DashboardLoadError("p", "m", null, null, "NotFound");
        var t = DateTimeOffset.UnixEpoch;

        var a = new DashboardLoadResult(null, err, t);
        var b = new DashboardLoadResult(null, err, t);

        a.Should().Be(b);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void DashboardLoadError_RecordEquality_WorksByValue()
    {
        var a = new DashboardLoadError("p", "m", 1, 2, "NotFound");
        var b = new DashboardLoadError("p", "m", 1, 2, "NotFound");

        a.Should().Be(b);
    }
}