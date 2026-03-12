using System.Numerics;
using WellAnalyzing.Application.Dto;
using WellAnalyzing.Application.UseCases.GetGroupingWells;
using WellAnalyzing.Domain.Enums;

namespace WellAnalyzing.Tests.Application.UseCases.GetGroupingWells;

public class GetGroupingWellsQueryHandlerTests
{
    private readonly GetGroupingWellsQueryHandler _sut;

    public GetGroupingWellsQueryHandlerTests()
    {
        _sut = new GetGroupingWellsQueryHandler();
    }

    public class Handle : GetGroupingWellsQueryHandlerTests
    {
        [Fact]
        public async Task WithSingleWellAndMultipleIntervals_CreatesWellWithAllIntervals()
        {
            var rawData = new List<RawWellDataDto>
            {
                new() { WellId = "A-001", X = 82.10f, Y = 55.20f, DepthFrom = 0, DepthTo = 10, Rock = "Sandstone", Porosity = 0.18f },
                new() { WellId = "A-001", X = 82.10f, Y = 55.20f, DepthFrom = 10, DepthTo = 25, Rock = "Limestone", Porosity = 0.07f },
                new() { WellId = "A-001", X = 82.10f, Y = 55.20f, DepthFrom = 25, DepthTo = 40, Rock = "Shale", Porosity = 0.04f }
            };

            var query = new GetGroupingWellsQuery(rawData);
            var result = await _sut.Handle(query, CancellationToken.None);

            Assert.Single(result);

            var well = result[0];

            Assert.Equal("A-001", well.Id);
            Assert.Equal(new Vector2(82.10f, 55.20f), well.Position);
            Assert.Equal(3, well.Intervals.Count);
            Assert.Equal(0, well.Intervals[0].DepthFrom);
            Assert.Equal(10, well.Intervals[1].DepthFrom);
            Assert.Equal(25, well.Intervals[2].DepthFrom);
        }

        [Fact]
        public async Task WithEmptyRawData_ReturnsEmptyArray()
        {
            var rawData = new List<RawWellDataDto>();
            var query = new GetGroupingWellsQuery(rawData);
            var result = await _sut.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task WithDifferentRockTypeStrings_ConvertsCorrectly()
        {
            var rawData = new List<RawWellDataDto>
            {
                new() { WellId = "A-001", X = 0, Y = 0, DepthFrom = 0, DepthTo = 10, Rock = "Sandstone", Porosity = 0.1f },
                new() { WellId = "A-002", X = 0, Y = 0, DepthFrom = 0, DepthTo = 10, Rock = "SANDSTONE", Porosity = 0.1f },
                new() { WellId = "A-003", X = 0, Y = 0, DepthFrom = 0, DepthTo = 10, Rock = "sandstone", Porosity = 0.1f },
                new() { WellId = "A-004", X = 0, Y = 0, DepthFrom = 0, DepthTo = 10, Rock = "Limestone", Porosity = 0.1f },
                new() { WellId = "A-005", X = 0, Y = 0, DepthFrom = 0, DepthTo = 10, Rock = "LIMESTONE", Porosity = 0.1f },
                new() { WellId = "A-006", X = 0, Y = 0, DepthFrom = 0, DepthTo = 10, Rock = "limestone", Porosity = 0.1f },
                new() { WellId = "A-007", X = 0, Y = 0, DepthFrom = 0, DepthTo = 10, Rock = "Shale", Porosity = 0.1f },
                new() { WellId = "A-008", X = 0, Y = 0, DepthFrom = 0, DepthTo = 10, Rock = "SHALE", Porosity = 0.1f },
                new() { WellId = "A-009", X = 0, Y = 0, DepthFrom = 0, DepthTo = 10, Rock = "shale", Porosity = 0.1f }
            };

            var query = new GetGroupingWellsQuery(rawData);
            var result = await _sut.Handle(query, CancellationToken.None);

            Assert.Equal(9, result.Length);
            Assert.Equal(RockType.Sandstone, result[0].Intervals[0].RockType);
            Assert.Equal(RockType.Sandstone, result[1].Intervals[0].RockType);
            Assert.Equal(RockType.Sandstone, result[2].Intervals[0].RockType);
            Assert.Equal(RockType.Limestone, result[3].Intervals[0].RockType);
            Assert.Equal(RockType.Limestone, result[4].Intervals[0].RockType);
            Assert.Equal(RockType.Limestone, result[5].Intervals[0].RockType);
            Assert.Equal(RockType.Shale, result[6].Intervals[0].RockType);
            Assert.Equal(RockType.Shale, result[7].Intervals[0].RockType);
            Assert.Equal(RockType.Shale, result[8].Intervals[0].RockType);
        }

        [Theory]
        [InlineData("Granite")]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        [InlineData("Unknown")]
        [InlineData("SAND")]
        public async Task WithUnknownRockType_ReturnsNull(string? rockType)
        {
            var rawData = new List<RawWellDataDto>
            {
                new() { WellId = "A-001", X = 0, Y = 0, DepthFrom = 0, DepthTo = 10, Rock = rockType, Porosity = 0.1f }
            };

            var query = new GetGroupingWellsQuery(rawData);
            var result = await _sut.Handle(query, CancellationToken.None);

            Assert.Single(result);
            Assert.Null(result[0].Intervals[0].RockType);
        }

        [Fact]
        public async Task WithMixedRockTypes_ConvertsEachIndependently()
        {
            var rawData = new List<RawWellDataDto>
            {
                new() { WellId = "A-001", X = 0, Y = 0, DepthFrom = 0, DepthTo = 10, Rock = "Sandstone", Porosity = 0.1f },
                new() { WellId = "A-001", X = 0, Y = 0, DepthFrom = 10, DepthTo = 20, Rock = "LIMESTONE", Porosity = 0.1f },
                new() { WellId = "A-001", X = 0, Y = 0, DepthFrom = 20, DepthTo = 30, Rock = "shale", Porosity = 0.1f },
                new() { WellId = "A-001", X = 0, Y = 0, DepthFrom = 30, DepthTo = 40, Rock = "Unknown", Porosity = 0.1f }
            };

            var query = new GetGroupingWellsQuery(rawData);
            var result = await _sut.Handle(query, CancellationToken.None);

            Assert.Single(result);

            var intervals = result[0].Intervals;

            Assert.Equal(4, intervals.Count);
            Assert.Equal(RockType.Sandstone, intervals[0].RockType);
            Assert.Equal(RockType.Limestone, intervals[1].RockType);
            Assert.Equal(RockType.Shale, intervals[2].RockType);
            Assert.Null(intervals[3].RockType);
        }

        [Fact]
        public async Task WithInconsistentCoordinates_UsesFirstIntervalCoordinates()
        {
            var rawData = new List<RawWellDataDto>
            {
                new() { WellId = "A-001", X = 82.10f, Y = 55.20f, DepthFrom = 0, DepthTo = 10, Rock = "Sandstone", Porosity = 0.1f },
                new() { WellId = "A-001", X = 83.00f, Y = 56.00f, DepthFrom = 10, DepthTo = 20, Rock = "Limestone", Porosity = 0.1f }, // другие координаты!
                new() { WellId = "A-001", X = 84.00f, Y = 57.00f, DepthFrom = 20, DepthTo = 30, Rock = "Shale", Porosity = 0.1f }      // снова другие!
            };

            var query = new GetGroupingWellsQuery(rawData);
            var result = await _sut.Handle(query, CancellationToken.None);

            Assert.Single(result);
            Assert.Equal(new Vector2(82.10f, 55.20f), result[0].Position);
            Assert.Equal(3, result[0].Intervals.Count);
        }

        [Fact]
        public async Task WithCancellationRequested_CompletesBeforeCancellation()
        {
            var rawData = new List<RawWellDataDto>
            {
                new() { WellId = "A-001", X = 82.10f, Y = 55.20f, DepthFrom = 0, DepthTo = 10, Rock = "Sandstone", Porosity = 0.1f },
                new() { WellId = "A-002", X = 90.00f, Y = 60.00f, DepthFrom = 0, DepthTo = 15, Rock = "Shale", Porosity = 0.1f }
            };

            var query = new GetGroupingWellsQuery(rawData);
            var cts = new CancellationTokenSource();
            var result = await _sut.Handle(query, cts.Token);

            Assert.Equal(2, result.Length);

            cts.Cancel();
        }

        [Fact]
        public async Task WithLargeDataset_HandlesEfficiently()
        {
            var rawData = new List<RawWellDataDto>();

            for (int i = 0; i < 100; i++)
            {
                string wellId = $"WELL-{i / 5:D3}";
                for (int j = 0; j < 5; j++)
                {
                    rawData.Add(new RawWellDataDto
                    {
                        WellId = wellId,
                        X = i * 1.0f,
                        Y = i * 2.0f,
                        DepthFrom = j * 10f,
                        DepthTo = (j + 1) * 10f,
                        Rock = "Sandstone",
                        Porosity = 0.15f
                    });
                }
            }

            var query = new GetGroupingWellsQuery(rawData);
            var result = await _sut.Handle(query, CancellationToken.None);

            Assert.Equal(20, result.Length); // 20 скважин

            foreach (var well in result)
            {
                Assert.Equal(25, well.Intervals.Count); // каждая по 5 интервалов
            }
        }

        [Fact]
        public async Task WithDataFromProvidedCsv_ConvertsCorrectly()
        {
            var rawData = new List<RawWellDataDto>
            {
                new() { WellId = "A-001", X = 82.10f, Y = 55.20f, DepthFrom = 0.00f, DepthTo = 10.00f, Rock = "Sandstone", Porosity = 0.18f },
                new() { WellId = "A-001", X = 82.10f, Y = 55.20f, DepthFrom = 10.00f, DepthTo = 25.00f, Rock = "Limestone", Porosity = 0.07f },
                new() { WellId = "A-002", X = 90.00f, Y = 60.00f, DepthFrom = 0.00f, DepthTo = 15.00f, Rock = "Shale", Porosity = 0.04f },
                new() { WellId = "A-002", X = 90.00f, Y = 60.00f, DepthFrom = 15.00f, DepthTo = 40.00f, Rock = "Sandstone", Porosity = 0.22f },
                new() { WellId = "A-003", X = 100.10f, Y = 72.50f, DepthFrom = 0.00f, DepthTo = 5.00f, Rock = "Sandstone", Porosity = 0.19f },
                new() { WellId = "A-003", X = 100.10f, Y = 72.50f, DepthFrom = 5.00f, DepthTo = 25.00f, Rock = null, Porosity = 2.00f },
                new() { WellId = "A-004", X = 110.30f, Y = 75.70f, DepthFrom = 0.00f, DepthTo = 15.00f, Rock = "Shale", Porosity = 0.50f },
                new() { WellId = "A-004", X = 110.30f, Y = 75.70f, DepthFrom = 15.00f, DepthTo = 30.00f, Rock = "Shale", Porosity = 0.45f },
                new() { WellId = "A-004", X = 110.30f, Y = 75.70f, DepthFrom = 30.00f, DepthTo = 45.00f, Rock = "Limestone", Porosity = 0.44f },
            };

            var query = new GetGroupingWellsQuery(rawData);
            var result = await _sut.Handle(query, CancellationToken.None);

            Assert.Equal(4, result.Length);

            var wellA003 = result.First(w => w.Id == "A-003");

            Assert.Equal(2, wellA003.Intervals.Count);
            Assert.Null(wellA003.Intervals[1].RockType);
            Assert.Equal(2.00f, wellA003.Intervals[1].Porosity);

            var wellA004 = result.First(w => w.Id == "A-004");

            Assert.Equal(3, wellA004.Intervals.Count);
            Assert.Equal(RockType.Shale, wellA004.Intervals[0].RockType);
            Assert.Equal(RockType.Shale, wellA004.Intervals[1].RockType);
            Assert.Equal(RockType.Limestone, wellA004.Intervals[2].RockType);
        }
    }
}