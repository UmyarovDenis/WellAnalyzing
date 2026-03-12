using MediatR;
using Moq;
using System.Numerics;
using WellAnalyzing.Application.Dto;
using WellAnalyzing.Application.Services.Interfaces;
using WellAnalyzing.Application.UseCases.GetWellSummaries;
using WellAnalyzing.Domain.Entities;
using WellAnalyzing.Domain.Enums;

namespace WellAnalyzing.Tests.Application.UseCases.GetWellSummaries;

public class GetWellSummariesQueryHandlerTests
{
    private readonly Mock<IWellAnalyzer> _wellAnalyzerMock;
    private readonly GetWellSummariesQueryHandler _sut;

    public GetWellSummariesQueryHandlerTests()
    {
        _wellAnalyzerMock = new Mock<IWellAnalyzer>();
        _sut = new GetWellSummariesQueryHandler(_wellAnalyzerMock.Object);
    }

    public class Handle : GetWellSummariesQueryHandlerTests
    {
        [Fact]
        public async Task WithMultipleWells_ReturnsSummariesForAllWells()
        {
            var wells = new[]
            {
                CreateWell("A-001", new Vector2(82.10f, 55.20f), 3),
                CreateWell("A-002", new Vector2(90.00f, 60.00f), 2),
                CreateWell("A-003", new Vector2(100.10f, 72.50f), 1)
            };

            var query = new GetWellSummariesQuery(wells);

            SetupWellAnalyzerMocks(wells[0], totalDepth: 40f, intervalCount: 3, porosityAvg: 0.125f, rock: "Sandstone");
            SetupWellAnalyzerMocks(wells[1], totalDepth: 30f, intervalCount: 2, porosityAvg: 0.15f, rock: "Limestone");
            SetupWellAnalyzerMocks(wells[2], totalDepth: 10f, intervalCount: 1, porosityAvg: 0.18f, rock: "Shale");

            var result = await _sut.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(3, result.Length);

            Assert.Equal("A-001", result[0].WellId);
            Assert.Equal(new Vector2(82.10f, 55.20f), result[0].Position);
            Assert.Equal(40f, result[0].TotalDepth);
            Assert.Equal(3, result[0].IntervalCount);
            Assert.Equal(0.125f, result[0].PorosityAverage);
            Assert.Equal("Sandstone", result[0].Rock);

            Assert.Equal("A-002", result[1].WellId);
            Assert.Equal(new Vector2(90.00f, 60.00f), result[1].Position);
            Assert.Equal(30f, result[1].TotalDepth);
            Assert.Equal(2, result[1].IntervalCount);
            Assert.Equal(0.15f, result[1].PorosityAverage);
            Assert.Equal("Limestone", result[1].Rock);

            Assert.Equal("A-003", result[2].WellId);
            Assert.Equal(new Vector2(100.10f, 72.50f), result[2].Position);
            Assert.Equal(10f, result[2].TotalDepth);
            Assert.Equal(1, result[2].IntervalCount);
            Assert.Equal(0.18f, result[2].PorosityAverage);
            Assert.Equal("Shale", result[2].Rock);

            VerifyWellAnalyzerCalls(wells[0], Times.Once());
            VerifyWellAnalyzerCalls(wells[1], Times.Once());
            VerifyWellAnalyzerCalls(wells[2], Times.Once());
        }

        [Fact]
        public async Task WithSingleWell_ReturnsSingleSummary()
        {
            var well = CreateWell("A-001", new Vector2(82.10f, 55.20f), 3);
            var wells = new[] { well };
            var query = new GetWellSummariesQuery(wells);

            SetupWellAnalyzerMocks(well, totalDepth: 40f, intervalCount: 3, porosityAvg: 0.125f, rock: "Sandstone");

            var result = await _sut.Handle(query, CancellationToken.None);

            Assert.Single(result);

            Assert.Equal("A-001", result[0].WellId);
            Assert.Equal(40f, result[0].TotalDepth);
            Assert.Equal(3, result[0].IntervalCount);
            Assert.Equal(0.125f, result[0].PorosityAverage);
            Assert.Equal("Sandstone", result[0].Rock);

            VerifyWellAnalyzerCalls(well, Times.Once());
        }

        [Fact]
        public async Task WithEmptyWellsArray_ReturnsEmptyArray()
        {
            var wells = Array.Empty<Well>();
            var query = new GetWellSummariesQuery(wells);
            var result = await _sut.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Empty(result);

            _wellAnalyzerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task WithWellHavingNoIntervals_HandlesGracefully()
        {
            var well = new Well("A-001", new Vector2(82.10f, 55.20f), Array.Empty<Interval>());
            var wells = new[] { well };
            var query = new GetWellSummariesQuery(wells);

            SetupWellAnalyzerMocks(well, totalDepth: 0f, intervalCount: 0, porosityAvg: 0f, rock: "Порода не указана");

            var result = await _sut.Handle(query, CancellationToken.None);

            Assert.Single(result);

            Assert.Equal("A-001", result[0].WellId);
            Assert.Equal(0f, result[0].TotalDepth);
            Assert.Equal(0, result[0].IntervalCount);
            Assert.Equal(0f, result[0].PorosityAverage);
            Assert.Equal("Порода не указана", result[0].Rock);
        }

        [Fact]
        public async Task WithRealWorldData_CalculatesCorrectSummaries()
        {
            var intervals1 = new List<Interval>
            {
                new(DepthFrom: 0, DepthTo: 10, RockType: RockType.Sandstone, Porosity: 0.18f),
                new(DepthFrom: 10, DepthTo: 25, RockType: RockType.Limestone, Porosity: 0.07f)
            };

            var intervals2 = new List<Interval>
            {
                new(DepthFrom: 0, DepthTo: 15, RockType: RockType.Shale, Porosity: 0.04f),
                new(DepthFrom: 15, DepthTo: 40, RockType: RockType.Sandstone, Porosity: 0.22f)
            };

            var intervals3 = new List<Interval>
            {
                new(DepthFrom: 0, DepthTo: 5, RockType: RockType.Sandstone, Porosity: 0.19f),
                new(DepthFrom: 5, DepthTo: 25, RockType: null, Porosity: 2.00f)
            };

            var wells = new[]
            {
                new Well("A-001", new Vector2(82.10f, 55.20f), intervals1),
                new Well("A-002", new Vector2(90.00f, 60.00f), intervals2),
                new Well("A-003", new Vector2(100.10f, 72.50f), intervals3)
            };

            var query = new GetWellSummariesQuery(wells);

            SetupWellAnalyzerMocks(wells[0], totalDepth: 25f, intervalCount: 2, porosityAvg: 0.125f, rock: "Sandstone");
            SetupWellAnalyzerMocks(wells[1], totalDepth: 40f, intervalCount: 2, porosityAvg: 0.13f, rock: "Sandstone");
            SetupWellAnalyzerMocks(wells[2], totalDepth: 25f, intervalCount: 2, porosityAvg: 0.19f, rock: "Порода не указана");

            var result = await _sut.Handle(query, CancellationToken.None);

            Assert.Equal(3, result.Length);

            Assert.Equal("A-001", result[0].WellId);
            Assert.Equal(25f, result[0].TotalDepth);
            Assert.Equal(2, result[0].IntervalCount);
            Assert.Equal(0.125f, result[0].PorosityAverage);
            Assert.Equal("Sandstone", result[0].Rock);

            Assert.Equal("A-002", result[1].WellId);
            Assert.Equal(40f, result[1].TotalDepth);
            Assert.Equal(2, result[1].IntervalCount);
            Assert.Equal(0.13f, result[1].PorosityAverage);
            Assert.Equal("Sandstone", result[1].Rock);

            Assert.Equal("A-003", result[2].WellId);
            Assert.Equal(25f, result[2].TotalDepth);
            Assert.Equal(2, result[2].IntervalCount);
            Assert.Equal(0.19f, result[2].PorosityAverage);
            Assert.Equal("Порода не указана", result[2].Rock);
        }

        [Fact]
        public async Task WithWellHavingSingleInterval_ReturnsCorrectSummary()
        {
            var intervals = new List<Interval>
            {
                new(DepthFrom: 0, DepthTo: 50, RockType: RockType.Sandstone, Porosity: 0.15f)
            };

            var well = new Well("A-001", new Vector2(82.10f, 55.20f), intervals);
            var wells = new[] { well };
            var query = new GetWellSummariesQuery(wells);

            SetupWellAnalyzerMocks(well, totalDepth: 50f, intervalCount: 1, porosityAvg: 0.15f, rock: "Sandstone");

            var result = await _sut.Handle(query, CancellationToken.None);

            Assert.Single(result);

            Assert.Equal(50f, result[0].TotalDepth);
            Assert.Equal(1, result[0].IntervalCount);
            Assert.Equal(0.15f, result[0].PorosityAverage);
            Assert.Equal("Sandstone", result[0].Rock);
        }

        [Fact]
        public async Task WithLargeNumberOfWells_ProcessesAll()
        {
            var wells = new List<Well>();
            for (int i = 0; i < 100; i++)
            {
                wells.Add(CreateWell($"WELL-{i:D3}", new Vector2(i, i * 2), 3));
            }

            var query = new GetWellSummariesQuery(wells.ToArray());

            foreach (var well in wells)
            {
                SetupWellAnalyzerMocks(well, totalDepth: 30f, intervalCount: 3, porosityAvg: 0.15f, rock: "Sandstone");
            }

            var result = await _sut.Handle(query, CancellationToken.None);

            Assert.Equal(100, result.Length);

            for (int i = 0; i < 100; i++)
            {
                Assert.Equal($"WELL-{i:D3}", result[i].WellId);
                Assert.Equal(30f, result[i].TotalDepth);
                Assert.Equal(3, result[i].IntervalCount);
                Assert.Equal(0.15f, result[i].PorosityAverage);
                Assert.Equal("Sandstone", result[i].Rock);
            }

            foreach (var well in wells)
            {
                VerifyWellAnalyzerCalls(well, Times.Once());
            }
        }

        [Fact]
        public async Task WhenWellAnalyzerThrows_PropagatesException()
        {
            var well = CreateWell("A-001", new Vector2(82.10f, 55.20f), 3);
            var wells = new[] { well };
            var query = new GetWellSummariesQuery(wells);

            var expectedException = new InvalidOperationException("Calculation failed");

            _wellAnalyzerMock
                .Setup(a => a.CalculateTotalDepth(well))
                .Throws(expectedException);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.Handle(query, CancellationToken.None));

            Assert.Equal(expectedException.Message, exception.Message);
        }

        [Fact]
        public async Task WithCancellationToken_CompletesSuccessfully()
        {
            var well = CreateWell("A-001", new Vector2(82.10f, 55.20f), 3);
            var wells = new[] { well };
            var query = new GetWellSummariesQuery(wells);

            var cts = new CancellationTokenSource();

            SetupWellAnalyzerMocks(well, totalDepth: 40f, intervalCount: 3, porosityAvg: 0.125f, rock: "Sandstone");

            var result = await _sut.Handle(query, cts.Token);

            Assert.Single(result);

            cts.Cancel();
        }

        [Fact]
        public async Task VerifyCorrectMapping_FromWellToSummaryDto()
        {
            var well = CreateWell("A-001", new Vector2(82.10f, 55.20f), 3);
            var wells = new[] { well };
            var query = new GetWellSummariesQuery(wells);

            _wellAnalyzerMock
                .Setup(a => a.CalculateTotalDepth(well))
                .Returns(42.5f);

            _wellAnalyzerMock
                .Setup(a => a.CalculateIntervalCount(well))
                .Returns(5);

            _wellAnalyzerMock
                .Setup(a => a.CalculatePorosityAverage(well))
                .Returns(0.275f);

            _wellAnalyzerMock
                .Setup(a => a.DetermineRock(well))
                .Returns("Limestone");

            var result = await _sut.Handle(query, CancellationToken.None);

            Assert.Single(result);

            Assert.Equal(well.Id, result[0].WellId);
            Assert.Equal(well.Position, result[0].Position);
            Assert.Equal(42.5f, result[0].TotalDepth);
            Assert.Equal(5, result[0].IntervalCount);
            Assert.Equal(0.275f, result[0].PorosityAverage);
            Assert.Equal("Limestone", result[0].Rock);
        }
    }

    private static Well CreateWell(string id, Vector2 position, int intervalCount)
    {
        var intervals = new List<Interval>();
        for (int i = 0; i < intervalCount; i++)
        {
            intervals.Add(new Interval(
                DepthFrom: i * 10f,
                DepthTo: (i + 1) * 10f,
                RockType: RockType.Sandstone,
                Porosity: 0.15f));
        }

        return new Well(id, position, intervals);
    }

    private void SetupWellAnalyzerMocks(Well well, float totalDepth, int intervalCount, float porosityAvg, string rock)
    {
        _wellAnalyzerMock
            .Setup(a => a.CalculateTotalDepth(well))
            .Returns(totalDepth);

        _wellAnalyzerMock
            .Setup(a => a.CalculateIntervalCount(well))
            .Returns(intervalCount);

        _wellAnalyzerMock
            .Setup(a => a.CalculatePorosityAverage(well))
            .Returns(porosityAvg);

        _wellAnalyzerMock
            .Setup(a => a.DetermineRock(well))
            .Returns(rock);
    }

    private void VerifyWellAnalyzerCalls(Well well, Times times)
    {
        _wellAnalyzerMock.Verify(a => a.CalculateTotalDepth(well), times);
        _wellAnalyzerMock.Verify(a => a.CalculateIntervalCount(well), times);
        _wellAnalyzerMock.Verify(a => a.CalculatePorosityAverage(well), times);
        _wellAnalyzerMock.Verify(a => a.DetermineRock(well), times);
    }
}