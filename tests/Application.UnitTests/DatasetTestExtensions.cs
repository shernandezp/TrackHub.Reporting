using TrackHub.Reporting.Domain.Models;

namespace TrackHub.Reporting.Application.UnitTests;

// Helpers to read a ReportDataset by column (property) name in unit tests.
internal static class DatasetTestExtensions
{
    public static int IndexOf(this ReportDataset dataset, string propertyName)
    {
        for (var i = 0; i < dataset.Columns.Count; i++)
        {
            if (dataset.Columns[i].PropertyName == propertyName)
            {
                return i;
            }
        }
        throw new ArgumentException($"Column '{propertyName}' not found.", nameof(propertyName));
    }

    public static object? Cell(this ReportDataset dataset, int row, string propertyName)
        => dataset.Rows[row][dataset.IndexOf(propertyName)];

    public static IEnumerable<object?> Column(this ReportDataset dataset, string propertyName)
    {
        var index = dataset.IndexOf(propertyName);
        return dataset.Rows.Select(r => r[index]);
    }
}
