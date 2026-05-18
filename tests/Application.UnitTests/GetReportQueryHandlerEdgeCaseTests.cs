using Moq;
using Common.Application.Interfaces;
using Common.Domain.Constants;
using TrackHub.Reporting.Application.Report.Queries.Get;
using TrackHub.Reporting.Domain.Interfaces;
using TrackHub.Reporting.Domain.Interfaces.Factory;
using TrackHub.Reporting.Domain.Models;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.UnitTests.Reports;

[TestFixture]
public class GetReportQueryHandlerEdgeCaseTests
{
    private Mock<IReportFactory> _reportFactoryMock;
    private Mock<IUser> _userMock;
    private Mock<IAccountFeatureReader> _featureReaderMock;
    private Mock<IReportAuditWriter> _reportAuditWriterMock;
    private GetReportQueryHandler _handler;
    private Mock<IReport> _reportMock;
    private Guid _accountId;

    [SetUp]
    public void SetUp()
    {
        _reportFactoryMock = new Mock<IReportFactory>();
        _userMock = new Mock<IUser>();
        _featureReaderMock = new Mock<IAccountFeatureReader>();
        _reportAuditWriterMock = new Mock<IReportAuditWriter>();
        _reportMock = new Mock<IReport>();
        _accountId = Guid.NewGuid();
        _userMock.Setup(u => u.AccountId).Returns(_accountId);
        _userMock.Setup(u => u.PrincipalType).Returns(PrincipalType.User);
        _userMock.Setup(u => u.UserId).Returns(Guid.NewGuid());
        _featureReaderMock.Setup(r => r.EnsureFeatureEnabledAsync(_accountId, FeatureKeys.Reports, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _reportAuditWriterMock.Setup(w => w.RecordReportExportAsync(_accountId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _handler = new GetReportQueryHandler(_reportFactoryMock.Object, _userMock.Object, _featureReaderMock.Object, _reportAuditWriterMock.Object);
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
            .ReturnsAsync(new ReportExportResult(Array.Empty<byte>(), 0));

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
            .ReturnsAsync(new ReportExportResult(largeBytes, 10_000));

        // Act
        var result = await _handler.Handle(new GetReportQuery(reportCode, filters), CancellationToken.None);

        // Assert
        Assert.That(result.Length, Is.EqualTo(10_000));
        Assert.That(result, Is.EqualTo(largeBytes));
    }
}

