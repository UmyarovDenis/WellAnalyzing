using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using System.Numerics;
using WellAnalyzing.Application.UseCases.GetValidationErrors;
using WellAnalyzing.Application.Validators;
using WellAnalyzing.Domain.Entities;
using WellAnalyzing.Domain.Enums;

namespace WellAnalyzing.Tests.Application.UseCases.GetValidationErrors;

public class GetValidationErrorsQueryHandlerTests
{
    private readonly Mock<IValidator<Well>> _validatorMock;
    private readonly GetValidationErrorsQueryHandler _sut;

    public GetValidationErrorsQueryHandlerTests()
    {
        _validatorMock = new Mock<IValidator<Well>>();
        _sut = new GetValidationErrorsQueryHandler(_validatorMock.Object);
    }

    public class Handle : GetValidationErrorsQueryHandlerTests
    {
        [Fact]
        public async Task WithValidWells_ReturnsNull()
        {
            var wells = new[]
            {
                CreateWell("A-001", 2),
                CreateWell("A-002", 3)
            };

            var query = new GetValidationErrorsQuery(wells);

            _validatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<Well>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var result = await _sut.Handle(query, CancellationToken.None);

            Assert.Null(result);

            _validatorMock.Verify(
                v => v.ValidateAsync(It.IsAny<ValidationContext<Well>>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task WithInvalidWells_ReturnsFormattedErrorsWithLineNumbers()
        {
            var well1 = CreateWell("A-001", 2);
            var well2 = CreateWell("A-002", 1);
            var wells = new[] { well1, well2 };
            var query = new GetValidationErrorsQuery(wells);

            _validatorMock
                .SetupSequence(v => v.ValidateAsync(It.IsAny<ValidationContext<Well>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateValidationResult(
                    ("[2] [A-001]", "Начальная глубина должна быть не менее нуля"),
                    ("[3] [A-001]", "Пористость должна находиться в диапазоне от 0 до 1")))
                .ReturnsAsync(CreateValidationResult(
                    ("[4] [A-002]", "Требуется указать тип породы")));

            var result = await _sut.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(3, result.Length);

            Assert.Contains("[2] [A-001]: Начальная глубина должна быть не менее нуля", result);
            Assert.Contains("[3] [A-001]: Пористость должна находиться в диапазоне от 0 до 1", result);
            Assert.Contains("[4] [A-002]: Требуется указать тип породы", result);
        }

        [Fact]
        public async Task WithMixedValidAndInvalidWells_ReturnsOnlyErrorsFromInvalidWells()
        {
            var well1 = CreateWell("A-001", 2);
            var well2 = CreateWell("A-002", 2);
            var well3 = CreateWell("A-003", 1);

            var wells = new[] { well1, well2, well3 };
            var query = new GetValidationErrorsQuery(wells);

            _validatorMock
                .SetupSequence(v => v.ValidateAsync(It.IsAny<ValidationContext<Well>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult())
                .ReturnsAsync(CreateValidationResult(
                    ("[4] [A-002]", "Начальная глубина должна быть не менее нуля"),
                    ("[5] [A-002]", "Требуется указать тип породы")))
                .ReturnsAsync(new ValidationResult());

            var result = await _sut.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(2, result.Length);
            Assert.Contains("[4] [A-002]: Начальная глубина должна быть не менее нуля", result);
            Assert.Contains("[5] [A-002]: Требуется указать тип породы", result);

            _validatorMock.Verify(
                v => v.ValidateAsync(It.IsAny<ValidationContext<Well>>(), It.IsAny<CancellationToken>()),
                Times.Exactly(3));
        }

        [Fact]
        public async Task WithEmptyWellsArray_ReturnsNull()
        {
            var wells = Array.Empty<Well>();
            var query = new GetValidationErrorsQuery(wells);
            var result = await _sut.Handle(query, CancellationToken.None);

            Assert.Null(result);

            _validatorMock.Verify(
                v => v.ValidateAsync(It.IsAny<ValidationContext<Well>>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task WithErrorsHavingNullCustomState_FiltersOutThem()
        {
            var well = CreateWell("A-001", 2);
            var wells = new[] { well };
            var query = new GetValidationErrorsQuery(wells);
            var validationResult = new ValidationResult(new[]
            {
                new ValidationFailure("DepthFrom", "Ошибка 1") { CustomState = "[2] [A-001]" },
                new ValidationFailure("Porosity", "Ошибка 2") { CustomState = null },
                new ValidationFailure("RockType", "Ошибка 3") { CustomState = "[3] [A-001]" }
            });

            _validatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<Well>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            var result = await _sut.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(2, result.Length);
            Assert.Contains("[2] [A-001]: Ошибка 1", result);
            Assert.Contains("[3] [A-001]: Ошибка 3", result);
            Assert.DoesNotContain("Ошибка 2", result);
        }

        [Fact]
        public async Task WithErrorsHavingNullErrorMessage_FiltersOutThem()
        {
            var well = CreateWell("A-001", 1);
            var wells = new[] { well };
            var query = new GetValidationErrorsQuery(wells);
            var validationResult = new ValidationResult(new[]
            {
                new ValidationFailure("DepthFrom", "Ошибка 1") { CustomState = "[2] [A-001]" },
                new ValidationFailure("Porosity", null!) { CustomState = "[2] [A-001]" },
                new ValidationFailure("RockType", "Ошибка 3") { CustomState = "[2] [A-001]" }
            });

            _validatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<Well>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            var result = await _sut.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(3, result.Length);
            Assert.Contains("[2] [A-001]: Ошибка 1", result);
            Assert.Contains("[2] [A-001]: Ошибка 3", result);
        }

        [Fact]
        public async Task WhenValidatorThrows_PropagatesException()
        {
            var well = CreateWell("A-001", 1);
            var wells = new[] { well };
            var query = new GetValidationErrorsQuery(wells);

            var expectedException = new InvalidOperationException("Validator failed");

            _validatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<Well>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.Handle(query, CancellationToken.None));

            Assert.Equal(expectedException.Message, exception.Message);
        }

        [Fact]
        public async Task WithRealWorldData_ReturnsExpectedErrors()
        {
            var intervals = new List<Interval>
            {
                new(DepthFrom: 0, DepthTo: 10, RockType: RockType.Sandstone, Porosity: 0.18f),
                new(DepthFrom: 10, DepthTo: 25, RockType: null, Porosity: 2.00f),
                new(DepthFrom: 30, DepthTo: 25, RockType: RockType.Limestone, Porosity: 0.12f)
            };

            var well = new Well("A-003", new Vector2(100.10f, 72.50f), intervals);
            var wells = new[] { well };
            var query = new GetValidationErrorsQuery(wells);
            var validationResult = new ValidationResult(new[]
            {
                new ValidationFailure("Porosity", "Пористость должна находиться в диапазоне от 0 до 1")
                {
                    CustomState = "[3] [A-003]"
                },
                new ValidationFailure("RockType", "Требуется указать тип породы")
                {
                    CustomState = "[3] [A-003]"
                },
                new ValidationFailure("DepthFrom", "Начальная глубина должна быть меньше конечной")
                {
                    CustomState = "[4] [A-003]"
                }
            });

            _validatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<Well>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            var result = await _sut.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(3, result.Length);
            Assert.Contains("[3] [A-003]: Пористость должна находиться в диапазоне от 0 до 1", result);
            Assert.Contains("[3] [A-003]: Требуется указать тип породы", result);
            Assert.Contains("[4] [A-003]: Начальная глубина должна быть меньше конечной", result);
        }
    }

    private static Well CreateWell(string id, int intervalCount)
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

        return new Well(id, new Vector2(0, 0), intervals);
    }

    private static ValidationResult CreateValidationResult(params (string CustomState, string ErrorMessage)[] errors)
    {
        var failures = errors.Select(e =>
        {
            var failure = new ValidationFailure("Property", e.ErrorMessage);
            failure.CustomState = e.CustomState;
            return failure;
        });

        return new ValidationResult(failures);
    }
}