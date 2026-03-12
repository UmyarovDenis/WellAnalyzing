using System.Numerics;
using WellAnalyzing.Application.Services;
using WellAnalyzing.Domain.Entities;
using WellAnalyzing.Domain.Enums;

namespace WellAnalyzing.Tests.Services;

public class WellAnalyzerTests
{
    private readonly WellAnalyzer _sut;

    public WellAnalyzerTests()
    {
        _sut = new WellAnalyzer();
    }

    public class CalculateIntervalCount : WellAnalyzerTests
    {
        [Fact]
        public void WithMultipleIntervals_ReturnsCorrectCount()
        {
            var well = new Well(
                Id: "A-001",
                Position: new Vector2(82.10f, 55.20f),
                Intervals: new List<Interval>
                {
                    new(DepthFrom: 0, DepthTo: 10, RockType: RockType.Sandstone, Porosity: 0.18f),
                    new(DepthFrom: 10, DepthTo: 25, RockType: RockType.Limestone, Porosity: 0.07f),
                    new(DepthFrom: 25, DepthTo: 40, RockType: RockType.Shale, Porosity: 0.04f)
                }
            );

            var result = _sut.CalculateIntervalCount(well);

            Assert.Equal(3, result);
        }

        [Fact]
        public void WithSingleInterval_ReturnsOne()
        {
            var well = new Well(
                Id: "A-002",
                Position: new Vector2(90.00f, 60.00f),
                Intervals: new List<Interval>
                {
                    new(DepthFrom: 0, DepthTo: 50, RockType: RockType.Sandstone, Porosity: 0.15f)
                }
            );

            var result = _sut.CalculateIntervalCount(well);

            Assert.Equal(1, result);
        }

        [Fact]
        public void WithEmptyIntervals_ReturnsZero()
        {
            var well = new Well(
                Id: "A-003",
                Position: new Vector2(100.10f, 72.50f),
                Intervals: new List<Interval>()
            );

            var result = _sut.CalculateIntervalCount(well);

            Assert.Equal(0, result);
        }
    }

    public class CalculatePorosityAverage : WellAnalyzerTests
    {
        [Fact]
        public void WithMultipleIntervals_ReturnsWeightedAverage()
        {
            var well = new Well(
                Id: "A-001",
                Position: new Vector2(82.10f, 55.20f),
                Intervals: new List<Interval>
                {
                    new(DepthFrom: 0, DepthTo: 10, RockType: RockType.Sandstone, Porosity: 0.18f),
                    new(DepthFrom: 10, DepthTo: 25, RockType: RockType.Limestone, Porosity: 0.07f),
                    new(DepthFrom: 25, DepthTo: 40, RockType: RockType.Shale, Porosity: 0.04f)
                }
            );

            var result = _sut.CalculatePorosityAverage(well);

            Assert.Equal(0.08625f, result, precision: 5);
        }

        [Fact]
        public void WithSingleInterval_ReturnsThatPorosity()
        {
            var well = new Well(
                Id: "A-002",
                Position: new Vector2(90.00f, 60.00f),
                Intervals: new List<Interval>
                {
                    new(DepthFrom: 0, DepthTo: 50, RockType: RockType.Sandstone, Porosity: 0.15f)
                }
            );

            var result = _sut.CalculatePorosityAverage(well);

            Assert.Equal(0.15f, result);
        }

        [Fact]
        public void WithIntervalsOfSameLength_ReturnsArithmeticMean()
        {
            var well = new Well(
                Id: "A-004",
                Position: new Vector2(110.30f, 75.70f),
                Intervals: new List<Interval>
                {
                    new(DepthFrom: 0, DepthTo: 10, RockType: RockType.Sandstone, Porosity: 0.10f),
                    new(DepthFrom: 10, DepthTo: 20, RockType: RockType.Limestone, Porosity: 0.20f),
                    new(DepthFrom: 20, DepthTo: 30, RockType: RockType.Shale, Porosity: 0.30f)
                }
            );

            var result = _sut.CalculatePorosityAverage(well);

            Assert.Equal(0.2f, result);
        }

        [Fact]
        public void WithZeroPorosityIntervals_ReturnsZero()
        {
            var well = new Well(
                Id: "A-005",
                Position: new Vector2(116.30f, 80.23f),
                Intervals: new List<Interval>
                {
                    new(DepthFrom: 0, DepthTo: 10, RockType: RockType.Sandstone, Porosity: 0f),
                    new(DepthFrom: 10, DepthTo: 20, RockType: RockType.Limestone, Porosity: 0f)
                }
            );

            var result = _sut.CalculatePorosityAverage(well);

            Assert.Equal(0f, result);
        }
    }

    public class CalculateTotalDepth : WellAnalyzerTests
    {
        [Fact]
        public void WithMultipleIntervals_ReturnsTotalDepth()
        {
            var well = new Well(
                Id: "A-001",
                Position: new Vector2(82.10f, 55.20f),
                Intervals: new List<Interval>
                {
                    new(DepthFrom: 0, DepthTo: 10, RockType: RockType.Sandstone, Porosity: 0.18f),
                    new(DepthFrom: 10, DepthTo: 25, RockType: RockType.Limestone, Porosity: 0.07f),
                    new(DepthFrom: 25, DepthTo: 40, RockType: RockType.Shale, Porosity: 0.04f)
                }
            );

            var result = _sut.CalculateTotalDepth(well);

            Assert.Equal(40f, result);
        }

        [Fact]
        public void WithSingleInterval_ReturnsIntervalLength()
        {
            var well = new Well(
                Id: "A-002",
                Position: new Vector2(90.00f, 60.00f),
                Intervals: new List<Interval>
                {
                    new(DepthFrom: 100, DepthTo: 150, RockType: RockType.Sandstone, Porosity: 0.15f)
                }
            );

            var result = _sut.CalculateTotalDepth(well);

            Assert.Equal(50f, result); // 150 - 100 = 50
        }

        [Fact]
        public void WithNonZeroStartDepth_ReturnsCorrectDifference()
        {
            var well = new Well(
                Id: "A-003",
                Position: new Vector2(100.10f, 72.50f),
                Intervals: new List<Interval>
                {
                    new(DepthFrom: 50, DepthTo: 75, RockType: RockType.Sandstone, Porosity: 0.19f),
                    new(DepthFrom: 75, DepthTo: 120, RockType: RockType.Limestone, Porosity: 0.07f)
                }
            );

            var result = _sut.CalculateTotalDepth(well);

            Assert.Equal(70f, result); // 120 - 50 = 70
        }

        [Fact]
        public void WithEmptyIntervals_ThrowsArgumentOutOfRangeException()
        {
            var well = new Well(
                Id: "A-004",
                Position: new Vector2(110.30f, 75.70f),
                Intervals: new List<Interval>()
            );

            Assert.Throws<ArgumentOutOfRangeException>(() => _sut.CalculateTotalDepth(well));
        }
    }

    public class DetermineRock : WellAnalyzerTests
    {
        [Fact]
        public void WithSingleRockType_ReturnsThatRock()
        {
            var well = new Well(
                Id: "A-001",
                Position: new Vector2(82.10f, 55.20f),
                Intervals: new List<Interval>
                {
                    new(DepthFrom: 0, DepthTo: 10, RockType: RockType.Sandstone, Porosity: 0.18f),
                    new(DepthFrom: 10, DepthTo: 25, RockType: RockType.Sandstone, Porosity: 0.07f),
                    new(DepthFrom: 25, DepthTo: 40, RockType: RockType.Sandstone, Porosity: 0.04f)
                }
            );

            var result = _sut.DetermineRock(well);

            Assert.Equal(RockType.Sandstone.ToString(), result);
        }

        [Fact]
        public void WithMultipleRockTypes_ReturnsMostCommonByDepth()
        {
            var well = new Well(
                Id: "A-002",
                Position: new Vector2(90.00f, 60.00f),
                Intervals: new List<Interval>
                {
                    new(DepthFrom: 0, DepthTo: 10, RockType: RockType.Sandstone, Porosity: 0.18f),   // глубина 10
                    new(DepthFrom: 10, DepthTo: 25, RockType: RockType.Limestone, Porosity: 0.07f),  // глубина 15
                    new(DepthFrom: 25, DepthTo: 45, RockType: RockType.Sandstone, Porosity: 0.22f),  // глубина 20
                    new(DepthFrom: 45, DepthTo: 50, RockType: RockType.Shale, Porosity: 0.04f)       // глубина 5
                }
            );

            var result = _sut.DetermineRock(well);

            Assert.Equal(RockType.Sandstone.ToString(), result);
        }

        [Fact]
        public void WithTieInDepth_ReturnsFirstMaxByDepth()
        {
            var well = new Well(
                Id: "A-003",
                Position: new Vector2(100.10f, 72.50f),
                Intervals: new List<Interval>
                {
                    new(DepthFrom: 0, DepthTo: 20, RockType: RockType.Sandstone, Porosity: 0.19f), // глубина 20
                    new(DepthFrom: 20, DepthTo: 40, RockType: RockType.Limestone, Porosity: 0.07f) // глубина 20
                }
            );

            var result = _sut.DetermineRock(well);

            Assert.Equal(RockType.Sandstone.ToString(), result);
        }

        [Fact]
        public void WithNullRockTypes_ReturnsRockNotSpecified()
        {
            var well = new Well(
                Id: "A-004",
                Position: new Vector2(110.30f, 75.70f),
                Intervals: new List<Interval>
                {
                    new(DepthFrom: 0, DepthTo: 10, RockType: null, Porosity: 0.18f),
                    new(DepthFrom: 10, DepthTo: 25, RockType: null, Porosity: 0.07f)
                }
            );

            var result = _sut.DetermineRock(well);

            Assert.Equal("Порода не указана", result);
        }

        [Fact]
        public void WithMixedNullAndNonNullRockTypes_IgnoresNulls()
        {
            var well = new Well(
                Id: "A-005",
                Position: new Vector2(116.30f, 80.23f),
                Intervals: new List<Interval>
                {
                    new(DepthFrom: 0, DepthTo: 10, RockType: null, Porosity: 0.18f),                // игнорируется
                    new(DepthFrom: 10, DepthTo: 25, RockType: RockType.Sandstone, Porosity: 0.22f), // глубина 15
                    new(DepthFrom: 25, DepthTo: 40, RockType: RockType.Limestone, Porosity: 0.07f), // глубина 15
                    new(DepthFrom: 40, DepthTo: 55, RockType: RockType.Sandstone, Porosity: 0.12f)  // глубина 15
                }
            );

            var result = _sut.DetermineRock(well);

            Assert.Equal(RockType.Sandstone.ToString(), result);
        }

        [Fact]
        public void WithEmptyIntervals_ReturnsRockNotSpecified()
        {
            var well = new Well(
                Id: "A-006",
                Position: new Vector2(120.00f, 85.00f),
                Intervals: new List<Interval>()
            );

            var result = _sut.DetermineRock(well);

            Assert.Equal("Порода не указана", result);
        }

        [Fact]
        public void WithAllRockTypesNull_ReturnsRockNotSpecified()
        {
            var well = new Well(
                Id: "A-007",
                Position: new Vector2(125.00f, 90.00f),
                Intervals: new List<Interval>
                {
                    new(DepthFrom: 0, DepthTo: 10, RockType: null, Porosity: 0.18f),
                    new(DepthFrom: 10, DepthTo: 25, RockType: null, Porosity: 0.07f),
                    new(DepthFrom: 25, DepthTo: 40, RockType: null, Porosity: 0.04f)
                }
            );

            var result = _sut.DetermineRock(well);

            Assert.Equal("Порода не указана", result);
        }
    }
}