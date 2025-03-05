using Moq;
using TrackHub.Reporting.Application.Report.Queries.Get;
using TrackHub.Reporting.Domain.Interfaces.Factory;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.UnitTests.Devices.Queries.GetByOperator;

[TestFixture]
public class GetReportQueryHandlerTests
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
    public async Task Handle_ShouldReturnReportBytes_WhenReportExists()
    {
        // Arrange
        var reportCode = "TestReport";
        var filters = new FilterDto();
        var query = new GetReportQuery(reportCode, filters);
        var cancellationToken = new CancellationToken();
        var expectedBytes = new byte[] { 1, 2, 3 };

        _reportFactoryMock.Setup(f => f.GetReport(reportCode)).Returns(_reportMock.Object);
        _reportMock.Setup(r => r.GenerateAsync(filters, cancellationToken)).ReturnsAsync(expectedBytes);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        Assert.That(expectedBytes, Is.EqualTo(result));
        _reportFactoryMock.Verify(f => f.GetReport(reportCode), Times.Once);
        _reportMock.Verify(r => r.GenerateAsync(filters, cancellationToken), Times.Once);
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
        _reportMock.Setup(r => r.GenerateAsync(filters, cancellationToken)).ReturnsAsync(expectedBytes);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        Assert.That(expectedBytes, Is.EqualTo(result));
        _reportFactoryMock.Verify(f => f.GetReport(reportCode), Times.Once);
        _reportMock.Verify(r => r.GenerateAsync(It.Is<FilterDto>(f => f == filters), cancellationToken), Times.Once);
    }
}
