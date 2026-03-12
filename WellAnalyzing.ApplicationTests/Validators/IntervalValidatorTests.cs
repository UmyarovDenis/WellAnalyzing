using FluentValidation;
using WellAnalyzing.Application.Validators;
using WellAnalyzing.Domain.Entities;
using WellAnalyzing.Domain.Enums;

namespace WellAnalyzing.Tests.Application.Validators;

public class IntervalValidatorTests
{
    private readonly IntervalValidator _sut;

    public IntervalValidatorTests()
    {
        _sut = new IntervalValidator();
    }

    public class Validate : IntervalValidatorTests
    {
        [Fact]
        public void WithValidInterval_ReturnsValid()
        {
            var interval = new Interval(
                DepthFrom: 10,
                DepthTo: 25,
                RockType: RockType.Sandstone,
                Porosity: 0.15f);

            var context = CreateContextWithState(interval, "5");
            var result = _sut.Validate(context);

            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void WithDepthFromNegative_ReturnsInvalidWithState()
        {
            var interval = new Interval(
                DepthFrom: -5,
                DepthTo: 25,
                RockType: RockType.Sandstone,
                Porosity: 0.15f);

            var context = CreateContextWithState(interval, "3");
            var result = _sut.Validate(context);

            Assert.False(result.IsValid);

            var error = Assert.Single(result.Errors);
            Assert.Equal("Начальная глубина должна быть не менее нуля", error.ErrorMessage);
            Assert.Equal(nameof(Interval.DepthFrom), error.PropertyName);
            Assert.Equal("3", error.CustomState);
        }

        [Fact]
        public void WithDepthFromGreaterThanDepthTo_ReturnsInvalidWithState()
        {
            var interval = new Interval(
                DepthFrom: 30,
                DepthTo: 25,
                RockType: RockType.Sandstone,
                Porosity: 0.15f);

            var context = CreateContextWithState(interval, "4");
            var result = _sut.Validate(context);

            Assert.False(result.IsValid);

            var error = Assert.Single(result.Errors);
            Assert.Equal("Начальная глубина должна быть меньше конечной", error.ErrorMessage);
            Assert.Equal(nameof(Interval.DepthFrom), error.PropertyName);
            Assert.Equal("4", error.CustomState);
        }

        [Fact]
        public void WithDepthFromEqualToDepthTo_ReturnsInvalid()
        {
            var interval = new Interval(
                DepthFrom: 25,
                DepthTo: 25,
                RockType: RockType.Sandstone,
                Porosity: 0.15f);

            var context = CreateContextWithState(interval, "5");
            var result = _sut.Validate(context);

            Assert.False(result.IsValid);

            var error = Assert.Single(result.Errors);
            Assert.Equal("Начальная глубина должна быть меньше конечной", error.ErrorMessage);
        }

        [Theory]
        [InlineData(-0.1f)]
        [InlineData(1.1f)]
        [InlineData(1.5f)]
        public void WithPorosityOutOfRange_ReturnsInvalidWithState(float porosity)
        {
            var interval = new Interval(
                DepthFrom: 10,
                DepthTo: 25,
                RockType: RockType.Sandstone,
                Porosity: porosity);

            var context = CreateContextWithState(interval, "6");
            var result = _sut.Validate(context);

            Assert.False(result.IsValid);

            var error = Assert.Single(result.Errors);
            Assert.Equal("Пористость должна находиться в диапазоне от 0 до 1", error.ErrorMessage);
            Assert.Equal(nameof(Interval.Porosity), error.PropertyName);
            Assert.Equal("6", error.CustomState);
        }

        [Theory]
        [InlineData(0.0f)]
        [InlineData(0.5f)]
        [InlineData(1.0f)]
        public void WithPorosityInRange_ReturnsValid(float porosity)
        {
            var interval = new Interval(
                DepthFrom: 10,
                DepthTo: 25,
                RockType: RockType.Sandstone,
                Porosity: porosity);

            var context = CreateContextWithState(interval, "7");
            var result = _sut.Validate(context);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void WithNullRockType_ReturnsInvalidWithState()
        {
            var interval = new Interval(
                DepthFrom: 10,
                DepthTo: 25,
                RockType: null,
                Porosity: 0.15f);

            var context = CreateContextWithState(interval, "8");
            var result = _sut.Validate(context);

            Assert.False(result.IsValid);

            var error = Assert.Single(result.Errors);
            Assert.Equal("Требуется указать тип породы", error.ErrorMessage);
            Assert.Equal(nameof(Interval.RockType), error.PropertyName);
            Assert.Equal("8", error.CustomState);
        }

        [Fact]
        public void WithMultipleValidationErrors_ReturnsAllErrorsWithCorrectStates()
        {
            var interval = new Interval(
                DepthFrom: -5,
                DepthTo: 10,
                RockType: null,
                Porosity: 1.5f);

            var context = CreateContextWithState(interval, "9");
            var result = _sut.Validate(context);

            Assert.False(result.IsValid);
            Assert.Equal(3, result.Errors.Count);

            foreach (var error in result.Errors)
            {
                Assert.Equal("9", error.CustomState);
            }

            var errorMessages = result.Errors.Select(e => e.ErrorMessage).ToList();
            Assert.Contains("Начальная глубина должна быть не менее нуля", errorMessages);
            Assert.Contains("Пористость должна находиться в диапазоне от 0 до 1", errorMessages);
            Assert.Contains("Требуется указать тип породы", errorMessages);
        }

        [Fact]
        public void WithoutStateInContext_ThrowsKeyNotFoundException()
        {
            var interval = new Interval(
                DepthFrom: -5,
                DepthTo: 25,
                RockType: RockType.Sandstone,
                Porosity: 0.15f);

            var context = new ValidationContext<Interval>(interval);
            var exception = Assert.Throws<KeyNotFoundException>(() => _sut.Validate(context));
        }

        [Fact]
        public void WithDepthFromZero_ReturnsValid()
        {
            var interval = new Interval(
                DepthFrom: 0,
                DepthTo: 25,
                RockType: RockType.Sandstone,
                Porosity: 0.15f);

            var context = CreateContextWithState(interval, "10");
            var result = _sut.Validate(context);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void WithDepthFromZeroAndDepthToZero_ReturnsInvalid()
        {
            var interval = new Interval(
                DepthFrom: 0,
                DepthTo: 0,
                RockType: RockType.Sandstone,
                Porosity: 0.15f);

            var context = CreateContextWithState(interval, "11");
            var result = _sut.Validate(context);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Начальная глубина должна быть меньше конечной");
        }

        [Theory]
        [InlineData(0, 10, 0.5, RockType.Sandstone, true)]
        [InlineData(-1, 10, 0.5, RockType.Sandstone, false)]
        [InlineData(10, 5, 0.5, RockType.Sandstone, false)]
        [InlineData(0, 10, 1.5, RockType.Sandstone, false)]
        [InlineData(0, 10, -0.1, RockType.Sandstone, false)]
        [InlineData(0, 10, 0.5, null, false)]
        public void ValidateAllRules(
            float depthFrom,
            float depthTo,
            float porosity,
            RockType? rockType,
            bool expectedIsValid)
        {
            // Arrange
            var interval = new Interval(
                DepthFrom: depthFrom,
                DepthTo: depthTo,
                RockType: rockType,
                Porosity: porosity);

            var context = CreateContextWithState(interval, "12");
            var result = _sut.Validate(context);

            Assert.Equal(expectedIsValid, result.IsValid);
        }

        [Fact]
        public void WithDepthFromGreaterThanDepthTo_ErrorMessageContainsCorrectValues()
        {
            var interval = new Interval(
                DepthFrom: 30,
                DepthTo: 25,
                RockType: RockType.Sandstone,
                Porosity: 0.15f);

            var context = CreateContextWithState(interval, "13");
            var result = _sut.Validate(context);
            Assert.False(result.IsValid);

            var error = result.Errors.First(e => e.ErrorMessage.Contains("меньше конечной"));

            Assert.NotNull(error);
            Assert.DoesNotContain("30", error.ErrorMessage);
            Assert.DoesNotContain("25", error.ErrorMessage);
        }

        [Fact]
        public void WithLargePrecisionPorosity_HandlesCorrectly()
        {
            var interval = new Interval(
                DepthFrom: 10,
                DepthTo: 25,
                RockType: RockType.Sandstone,
                Porosity: 0.123456789f);

            var context = CreateContextWithState(interval, "14");
            var result = _sut.Validate(context);

            Assert.True(result.IsValid);
        }
    }

    private static ValidationContext<Interval> CreateContextWithState(Interval interval, string state)
    {
        var context = new ValidationContext<Interval>(interval);
        var stateMap = new Dictionary<Interval, string>
        {
            [interval] = state
        };
        context.RootContextData[ValidationContextDataKeys.RawDataLine] = stateMap;
        return context;
    }
}