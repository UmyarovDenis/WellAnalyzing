using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using WellAnalyzing.Application.Dto;
using WellAnalyzing.Application.Services;
using WellAnalyzing.Application.UseCases.GetGroupingWells;
using WellAnalyzing.Application.UseCases.GetValidationErrors;
using WellAnalyzing.Application.UseCases.GetWellSummaries;
using WellAnalyzing.Domain.Entities;
using System.Numerics;

namespace WellAnalyzing.Tests.Application.Services;

public class WellReportSummaryServiceTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<WellReportSummaryService>> _loggerMock;
    private readonly WellReportSummaryService _sut;

    public WellReportSummaryServiceTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<WellReportSummaryService>>();
        _sut = new WellReportSummaryService(_mediatorMock.Object, _loggerMock.Object);
    }

    public class CreateWellReportAsync : WellReportSummaryServiceTests
    {
        [Fact]
        public async Task WithValidData_ReturnsWellReportWithSummariesAndErrors()
        {
            var rawData = new List<RawWellDataDto>
            {
                new() { WellId = "A-001", X = 82.10f, Y = 55.20f, DepthFrom = 0, DepthTo = 10, Rock = "Sandstone", Porosity = 0.18f },
                new() { WellId = "A-001", X = 82.10f, Y = 55.20f, DepthFrom = 10, DepthTo = 25, Rock = "Limestone", Porosity = 0.07f },
                new() { WellId = "A-002", X = 90.00f, Y = 60.00f, DepthFrom = 0, DepthTo = 15, Rock = "Shale", Porosity = 0.04f }
            };

            var expectedWells = new[]
            {
                new Well("A-001", new Vector2(82.10f, 55.20f), new List<Interval>()),
                new Well("A-002", new Vector2(90.00f, 60.00f), new List<Interval>())
            };

            var expectedValidationErrors = new[] { "Ошибка в скважине A-001" };

            var expectedWellSummaries = new[]
            {
                new WellSummaryDto("A-001", new Vector2(82.10f, 55.20f), 25f, 2, 0.125f, "Sandstone"),
                new WellSummaryDto("A-002", new Vector2(90.00f, 60.00f), 15f, 1, 0.04f, "Shale")
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetGroupingWellsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedWells);

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetValidationErrorsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedValidationErrors);

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetWellSummariesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedWellSummaries);

            var result = await _sut.CreateWellReportAsync(rawData);

            Assert.NotNull(result);
            Assert.Equal(expectedWellSummaries, result.WellSummaries);
            Assert.Equal(expectedValidationErrors, result.ValidationErrors);

            _mediatorMock.Verify(
                m => m.Send(
                    It.Is<GetGroupingWellsQuery>(q => q.RawWells == rawData),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _mediatorMock.Verify(
                m => m.Send(
                    It.Is<GetValidationErrorsQuery>(q => q.Wells == expectedWells),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _mediatorMock.Verify(
                m => m.Send(
                    It.Is<GetWellSummariesQuery>(q => q.WellSummaryDtos == expectedWells),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task WithEmptyRawData_ReturnsReportWithEmptyCollections()
        {
            var rawData = new List<RawWellDataDto>();

            var expectedWells = Array.Empty<Well>();
            var expectedValidationErrors = Array.Empty<string>();
            var expectedWellSummaries = Array.Empty<WellSummaryDto>();

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetGroupingWellsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedWells);

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetValidationErrorsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedValidationErrors);

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetWellSummariesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedWellSummaries);

            var result = await _sut.CreateWellReportAsync(rawData);

            Assert.NotNull(result);
            Assert.Empty(result.WellSummaries);
            Assert.Empty(result.ValidationErrors);
        }

        [Fact]
        public async Task WhenGetGroupingWellsThrows_LogsErrorAndRethrows()
        {
            var rawData = new List<RawWellDataDto>
            {
                new() { WellId = "A-001", X = 82.10f, Y = 55.20f, DepthFrom = 0, DepthTo = 10 }
            };

            var expectedException = new InvalidOperationException("Grouping failed");

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetGroupingWellsQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.CreateWellReportAsync(rawData));

            Assert.Equal(expectedException.Message, exception.Message);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    expectedException,
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);

            _mediatorMock.Verify(
                m => m.Send(It.IsAny<GetValidationErrorsQuery>(), It.IsAny<CancellationToken>()),
                Times.Never);

            _mediatorMock.Verify(
                m => m.Send(It.IsAny<GetWellSummariesQuery>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task WhenGetValidationErrorsThrows_LogsErrorAndRethrows()
        {
            var rawData = new List<RawWellDataDto>
            {
                new() { WellId = "A-001", X = 82.10f, Y = 55.20f, DepthFrom = 0, DepthTo = 10 }
            };

            var expectedWells = new[]
            {
                new Well("A-001", new Vector2(82.10f, 55.20f), new List<Interval>())
            };

            var expectedException = new InvalidOperationException("Validation failed");

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetGroupingWellsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedWells);

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetValidationErrorsQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.CreateWellReportAsync(rawData));

            Assert.Equal(expectedException.Message, exception.Message);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    expectedException,
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);

            _mediatorMock.Verify(
                m => m.Send(It.IsAny<GetWellSummariesQuery>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task WhenGetWellSummariesThrows_LogsErrorAndRethrows()
        {
            var rawData = new List<RawWellDataDto>
            {
                new() { WellId = "A-001", X = 82.10f, Y = 55.20f, DepthFrom = 0, DepthTo = 10 }
            };

            var expectedWells = new[]
            {
                new Well("A-001", new Vector2(82.10f, 55.20f), new List<Interval>())
            };

            var expectedValidationErrors = Array.Empty<string>();

            var expectedException = new InvalidOperationException("Summaries failed");

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetGroupingWellsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedWells);

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetValidationErrorsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedValidationErrors);

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetWellSummariesQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.CreateWellReportAsync(rawData));

            Assert.Equal(expectedException.Message, exception.Message);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    expectedException,
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task WhenValidationErrorsReturnsNull_ReturnsReportWithNullErrors()
        {
            var rawData = new List<RawWellDataDto>
            {
                new() { WellId = "A-001", X = 82.10f, Y = 55.20f, DepthFrom = 0, DepthTo = 10 }
            };

            var expectedWells = new[]
            {
                new Well("A-001", new Vector2(82.10f, 55.20f), new List<Interval>())
            };

            var expectedWellSummaries = new[]
            {
                new WellSummaryDto("A-001", new Vector2(82.10f, 55.20f), 10f, 1, 0.18f, "Sandstone")
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetGroupingWellsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedWells);

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetValidationErrorsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string[]?)null);

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetWellSummariesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedWellSummaries);

            var result = await _sut.CreateWellReportAsync(rawData);

            Assert.NotNull(result);
            Assert.Null(result.ValidationErrors);
            Assert.Equal(expectedWellSummaries, result.WellSummaries);
        }

        [Fact]
        public async Task WhenRawDataIsLarge_PassesDataCorrectly()
        {
            var rawData = new List<RawWellDataDto>();
            for (int i = 0; i < 1000; i++)
            {
                rawData.Add(new RawWellDataDto
                {
                    WellId = $"WELL-{i:D4}",
                    X = i * 1.0f,
                    Y = i * 2.0f,
                    DepthFrom = 0,
                    DepthTo = 10,
                    Rock = "Sandstone",
                    Porosity = 0.15f
                });
            }

            GetGroupingWellsQuery? capturedQuery = null;
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetGroupingWellsQuery>(), It.IsAny<CancellationToken>()))
                .Callback<IRequest<Well[]>, CancellationToken>((q, _) => capturedQuery = q as GetGroupingWellsQuery)
                .ReturnsAsync(Array.Empty<Well>());

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetValidationErrorsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<string>());

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetWellSummariesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<WellSummaryDto>());

            await _sut.CreateWellReportAsync(rawData);

            Assert.NotNull(capturedQuery);
            Assert.Equal(rawData, capturedQuery!.RawWells);
        }
    }
}