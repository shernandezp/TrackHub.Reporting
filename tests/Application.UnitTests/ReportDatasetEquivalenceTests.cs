using TrackHub.Reporting.Domain.Models;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.UnitTests;

// Dataset-equivalence tests: for a representative VM per report family, the dataset's
// columns must equal the VM's public property names in declaration order, and each row must carry the
// same values the pre-refactor ClosedXML pipeline exported (it reflected the same properties).
[TestFixture]
public class ReportDatasetEquivalenceTests
{
    private static FilterDto Filters() => new()
    {
        Name = "My Report",
        Language = "en",
        DateTimeFilter1 = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
        DateTimeFilter2 = new DateTimeOffset(2026, 1, 31, 0, 0, 0, TimeSpan.Zero),
        StringFilter1 = "abc",
        NumericFilter1 = 7
    };

    private static void AssertMatchesProperties<T>(IReadOnlyCollection<T> rows)
    {
        var properties = typeof(T).GetProperties();
        var dataset = ReportDataset.Create(Filters(), rows);

        // Columns == property names, in declaration order.
        Assert.That(dataset.Columns.Select(c => c.PropertyName), Is.EqualTo(properties.Select(p => p.Name)));
        Assert.That(dataset.Columns.Select(c => c.PropertyType), Is.EqualTo(properties.Select(p => p.PropertyType)));

        // Rows carry the same values the old pipeline projected.
        Assert.That(dataset.RowCount, Is.EqualTo(rows.Count));
        var rowList = rows.ToList();
        for (var r = 0; r < rowList.Count; r++)
        {
            for (var c = 0; c < properties.Length; c++)
            {
                Assert.That(dataset.Rows[r][c], Is.EqualTo(properties[c].GetValue(rowList[r])));
            }
        }

        // Title + date range come from the filter.
        Assert.That(dataset.Title, Is.EqualTo("My Report"));
        Assert.That(dataset.FromDate, Is.EqualTo(Filters().DateTimeFilter1));
        Assert.That(dataset.ToDate, Is.EqualTo(Filters().DateTimeFilter2));
    }

    [Test]
    public void Legacy_PositionVm_ColumnsAndValuesMatch()
        => AssertMatchesProperties(new[]
        {
            new PositionVm(Guid.NewGuid(), "Dev", "Truck", 4.5, -74.1, 10.0,
                DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, 60, 90, 1, "addr", "city", "state", "country", null)
        });

    [Test]
    public void Gps_ProviderHealthRowVm_ColumnsAndValuesMatch()
        => AssertMatchesProperties(new[]
        {
            new GpsProviderHealthRowVm(Guid.NewGuid(), "Op1", "Healthy", 0.99, 25.0, 0, DateTimeOffset.UtcNow, null)
        });

    [Test]
    public void Admin_AccountByStatusRowVm_ColumnsAndValuesMatch()
        => AssertMatchesProperties(new[]
        {
            new AccountByStatusRowVm("Acme", "Active", 1, true, DateTimeOffset.UtcNow)
        });

    [Test]
    public void Document_ExpiringDocumentRowVm_ColumnsAndValuesMatch()
        => AssertMatchesProperties(new[]
        {
            new ExpiringDocumentRowVm("SOAT", "Transporter", "t1", "a.pdf", "Internal", "Active", DateTimeOffset.UtcNow)
        });

    [Test]
    public void AppliedFilters_IncludeNonEmptyFilterValues()
    {
        var dataset = ReportDataset.Create(Filters(), new[] { new AccountByStatusRowVm("A", "Active", 1, true, DateTimeOffset.UtcNow) });
        var keys = dataset.AppliedFilters.Select(f => f.Key).ToList();

        Assert.That(keys, Does.Contain("FilterFrom"));
        Assert.That(keys, Does.Contain("FilterTo"));
        Assert.That(keys, Does.Contain("Filter")); // StringFilter1 + NumericFilter1
    }
}
