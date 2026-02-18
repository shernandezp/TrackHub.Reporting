using Moq;
using TrackHub.Reporting.Application.Report.Queries.Get;
using TrackHub.Reporting.Domain.Interfaces.Factory;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.UnitTests.Reports;

[TestFixture]
public class GetReportQueryHandlerEdgeCaseTests
{
    private Mock<IReportFactory> _reportFactoryMock;
    private GetReportQueryHandler _handler;
    private Mock<IReport> _reportMock;

    [SetUp]
    public void SetUp()
    {
        _reportFactoryMock = new Mock<IReportFactory>();
        _reportMock = new Mock<IReport>();
        _handler = new GetReportQueryHandler(_reportFactoryMock.Object);
    }

    [Test]
    public async Task Handle_ReportReturnsEmptyBytes_ReturnsEmptyArray()
    {
        // Arrange — valid report but no data
        var reportCode = "EmptyReport";
        var filters = new FilterDto();
        var query = new GetReportQuery(reportCode, filters);

        _reportFactoryMock.Setup(f => f.GetReport(reportCode)).Returns(_reportMock.Object);
        _reportMock.Setup(r => r.GenerateAsync(filters, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<byte>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Handle_ReportGenerateThrows_PropagatesException()
    {
        // Arrange — factory returns report but Generate fails
        var reportCode = "FailingReport";
        var filters = new FilterDto();

        _reportFactoryMock.Setup(f => f.GetReport(reportCode)).Returns(_reportMock.Object);
        _reportMock.Setup(r => r.GenerateAsync(filters, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database unavailable"));

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _handler.Handle(new GetReportQuery(reportCode, filters), CancellationToken.None));
    }

    [Test]
    public async Task Handle_LargeReport_ReturnsAllBytes()
    {
        // Arrange — simulate a large report
        var reportCode = "LargeReport";
        var filters = new FilterDto();
        var largeBytes = new byte[10_000];
        new Random(42).NextBytes(largeBytes);

        _reportFactoryMock.Setup(f => f.GetReport(reportCode)).Returns(_reportMock.Object);
        _reportMock.Setup(r => r.GenerateAsync(filters, It.IsAny<CancellationToken>()))
            .ReturnsAsync(largeBytes);

        // Act
        var result = await _handler.Handle(new GetReportQuery(reportCode, filters), CancellationToken.None);

        // Assert
        Assert.That(result.Length, Is.EqualTo(10_000));
        Assert.That(result, Is.EqualTo(largeBytes));
    }
}
