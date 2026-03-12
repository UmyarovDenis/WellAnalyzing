using FluentValidation;
using System.Numerics;
using WellAnalyzing.Application.Validators;
using WellAnalyzing.Domain.Entities;
using WellAnalyzing.Domain.Enums;

namespace WellAnalyzing.Tests.Application.Validators;

public class WellValidatorTests
{
    private readonly WellValidator _sut;

    public WellValidatorTests()
    {
        _sut = new WellValidator();
    }

    public class Validate : WellValidatorTests
    {
        [Fact]
        public void WithNonIntersectingIntervals_ReturnsValid()
        {
            var intervals = new List<Interval>
            {
                new(DepthFrom: 0, DepthTo: 10, RockType: RockType.Sandstone, Porosity: 0.18f),
                new(DepthFrom: 10, DepthTo: 25, RockType: RockType.Limestone, Porosity: 0.07f),
                new(DepthFrom: 25, DepthTo: 40, RockType: RockType.Shale, Porosity: 0.04f)
            };

            var well = new Well("A-001", new Vector2(82.10f, 55.20f), intervals);

            var result = _sut.Validate(well);

            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void WithAdjacentIntervals_ReturnsValid()
        {
            var intervals = new List<Interval>
            {
                new(DepthFrom: 0, DepthTo: 10, RockType: RockType.Sandstone, Porosity: 0.18f),
                new(DepthFrom: 10, DepthTo: 25, RockType: RockType.Limestone, Porosity: 0.07f),
                new(DepthFrom: 25, DepthTo: 40, RockType: RockType.Shale, Porosity: 0.04f)
            };

            var well = new Well("A-001", new Vector2(82.10f, 55.20f), intervals);

            var result = _sut.Validate(well);

            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void WithSingleInterval_ReturnsValid()
        {
            var intervals = new List<Interval>
            {
                new(DepthFrom: 0, DepthTo: 50, RockType: RockType.Sandstone, Porosity: 0.15f)
            };

            var well = new Well("A-002", new Vector2(90.00f, 60.00f), intervals);

            var result = _sut.Validate(well);

            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void WithEmptyIntervals_ReturnsValid()
        {
            var intervals = new List<Interval>();
            var well = new Well("A-003", new Vector2(100.10f, 72.50f), intervals);

            var result = _sut.Validate(well);

            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void WithIntersectingIntervalsAndMissingState_ThrowsKeyNotFoundException()
        {
            var intervals = new List<Interval>
            {
                new(DepthFrom: 0, DepthTo: 15, RockType: RockType.Sandstone, Porosity: 0.18f),
                new(DepthFrom: 10, DepthTo: 25, RockType: RockType.Limestone, Porosity: 0.07f)
            };

            var well = new Well("A-001", new Vector2(82.10f, 55.20f), intervals);

            var context = new ValidationContext<Well>(well);
            var exception = Assert.Throws<KeyNotFoundException>(() => _sut.Validate(context));
        }
    }
}