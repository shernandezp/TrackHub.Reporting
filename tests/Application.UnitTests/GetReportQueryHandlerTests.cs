using Moq;
using Common.Application.Interfaces;
using Common.Domain.Constants;
using TrackHub.Reporting.Application.Report.Queries.Get;
using TrackHub.Reporting.Domain.Interfaces.Foundation;
using TrackHub.Reporting.Domain.Interfaces.Factory;
using TrackHub.Reporting.Domain.Models;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.UnitTests.Devices.Queries.GetByOperator;

[TestFixture]
public class GetReportQueryHandlerTests
{
    private Mock<IReportFactory> _reportFactoryMock;
    private Mock<IUser> _userMock;
    private Mock<IPlatformFeatureReader> _featureReaderMock;
    private Mock<IReportAuditWriter> _reportAuditWriterMock;
    private GetReportQueryHandler _handler;
    private Mock<IReport> _reportMock;
    private Guid _accountId;

    [SetUp]
    public void SetUp()
    {
        _reportFactoryMock = new Mock<IReportFactory>();
        _userMock = new Mock<IUser>();
        _featureReaderMock = new Mock<IPlatformFeatureReader>();
        _reportAuditWriterMock = new Mock<IReportAuditWriter>();
        _reportMock = new Mock<IReport>();
        _accountId = Guid.NewGuid();
        _userMock.Setup(u => u.AccountId).Returns(_accountId);
        _userMock.Setup(u => u.PrincipalType).Returns(PrincipalType.User);
        _userMock.Setup(u => u.UserId).Returns(Guid.NewGuid());
        _userMock.Setup(u => u.CorrelationId).Returns("test-correlation");
        _featureReaderMock.Setup(r => r.EnsureFeatureEnabledAsync(_accountId, FeatureKeys.Reports, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _reportAuditWriterMock.Setup(w => w.RecordReportExportAsync(_accountId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _handler = new GetReportQueryHandler(_reportFactoryMock.Object, _userMock.Object, _featureReaderMock.Object, _reportAuditWriterMock.Object);
    }

    [Test]
    public async Task Handle_ShouldReturnReportBytes_WhenReportExists()
    {
        // Arrange
        var reportCode = "TestReport";
        var filters = new FilterDto();
        var query = new GetReportQuery(reportCode, filters);
        var cancellationToken = new CancellationToken();
        var expectedBytes = new byte[] { 1, 2, 3 };

        _reportFactoryMock.Setup(f => f.GetReport(reportCode)).Returns(_reportMock.Object);
        _reportMock.Setup(r => r.GenerateAsync(filters, cancellationToken)).ReturnsAsync(new ReportExportResult(expectedBytes, 3));

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        Assert.That(expectedBytes, Is.EqualTo(result));
        _featureReaderMock.Verify(r => r.EnsureFeatureEnabledAsync(_accountId, FeatureKeys.Reports, cancellationToken), Times.Once);
        _reportFactoryMock.Verify(f => f.GetReport(reportCode), Times.Once);
        _reportMock.Verify(r => r.GenerateAsync(filters, cancellationToken), Times.Once);
        _reportAuditWriterMock.Verify(w => w.RecordReportExportAsync(_accountId, "User", It.IsAny<string>(), reportCode, It.IsAny<string>(), 3, "xlsx", "test-correlation", cancellationToken), Times.Once);
    }

    [Test]
    public void Handle_ShouldThrowException_WhenReportDoesNotExist()
    {
        // Arrange
        var reportCode = "NonExistentReport";
        var filters = new FilterDto();
        var query = new GetReportQuery(reportCode, filters);
        var cancellationToken = new CancellationToken();

        _reportFactoryMock.Setup(f => f.GetReport(reportCode)).Throws(new KeyNotFoundException());

        // Act & Assert
        Assert.ThrowsAsync<KeyNotFoundException>(async () => await _handler.Handle(query, cancellationToken));
        _featureReaderMock.Verify(r => r.EnsureFeatureEnabledAsync(_accountId, FeatureKeys.Reports, cancellationToken), Times.Once);
        _reportFactoryMock.Verify(f => f.GetReport(reportCode), Times.Once);
    }

    [Test]
    public async Task Handle_ShouldPassCorrectParametersToGenerateAsync()
    {
        // Arrange
        var reportCode = "TestReport";
        var filters = new FilterDto();
        var query = new GetReportQuery(reportCode, filters);
        var cancellationToken = new CancellationToken();
        var expectedBytes = new byte[] { 1, 2, 3 };

        _reportFactoryMock.Setup(f => f.GetReport(reportCode)).Returns(_reportMock.Object);
        _reportMock.Setup(r => r.GenerateAsync(filters, cancellationToken)).ReturnsAsync(new ReportExportResult(expectedBytes, 3));

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        Assert.That(expectedBytes, Is.EqualTo(result));
        _featureReaderMock.Verify(r => r.EnsureFeatureEnabledAsync(_accountId, FeatureKeys.Reports, cancellationToken), Times.Once);
        _reportFactoryMock.Verify(f => f.GetReport(reportCode), Times.Once);
        _reportMock.Verify(r => r.GenerateAsync(It.Is<FilterDto>(f => f == filters), cancellationToken), Times.Once);
        _reportAuditWriterMock.Verify(w => w.RecordReportExportAsync(_accountId, "User", It.IsAny<string>(), reportCode, It.IsAny<string>(), 3, "xlsx", "test-correlation", cancellationToken), Times.Once);
    }

    [Test]
    public void Handle_ShouldThrowUnauthorizedAccessException_WhenAccountClaimIsMissing()
    {
        _userMock.Setup(u => u.AccountId).Returns((Guid?)null);

        Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(new GetReportQuery("TestReport", new FilterDto()), CancellationToken.None));

        _featureReaderMock.Verify(r => r.EnsureFeatureEnabledAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _reportFactoryMock.Verify(f => f.GetReport(It.IsAny<string>()), Times.Never);
    }
}
